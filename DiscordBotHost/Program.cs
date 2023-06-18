
using DiscordBotHost.EntityFramework;

using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.CreateLogger();

IConfiguration config = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", false, true)
	.AddJsonFile("appsettings.secret.json", true, true)
	.AddEnvironmentVariables()
	.Build();

var services = new ServiceCollection()
	.AddSingleton(config)
	.AddSingleton(new OpenAIClient(new(config["OPENAI_API_KEY"], config["OPENAI_ORGANIZATION_ID"])))
	.AddSingleton(sp =>
	{
		DiscordSocketClient client = new(new()
		{
			AlwaysDownloadUsers = true,
			MessageCacheSize = 100,
			GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
			LogLevel = LogSeverity.Info
		});

		return client;
	})
	.AddSingleton<DiscordEventListener>()
	.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
	.AddScoped(sp =>
	{
		var guid = Guid.NewGuid();
		return new Func<Guid>(() => guid);
	})
	.AddSingleton(_ => new QueueClient(config["AZURE_STORAGE"], "shares"))
	.AddSingleton<AndroidShareQueueListener>()
	.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<Program>())
	.AddDbContext<DiscordBotDbContext>(options =>
	{
		options.UseSqlServer(config["MSSQL_CONNECTIONSTRING"]);
	})
	.BuildServiceProvider();

Log.Information("Migration of database started.");
using (var scope = services.CreateScope())
{
	await scope.ServiceProvider.GetRequiredService<DiscordBotDbContext>().Database.MigrateAsync();
}
Log.Information("Migration finished.");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
	Log.Information("Ctrl+C pressed, shutting down.");
	cts.Cancel();
	e.Cancel = true; // Prevent the process from terminating immediately
};

DiscordEventListener? discordListener = default;
try
{
	discordListener = services.GetRequiredService<DiscordEventListener>();
	await discordListener.StartAsync(config["DISCORD_TOKEN"] ??
		throw new InvalidOperationException("DISCORD_TOKEN is a required configuration."));

	var storageQueueListener = services.GetRequiredService<AndroidShareQueueListener>();
	//await storageQueueListener.StartAsync(cts.Token);

	Log.Information(" ------------------------------");
	Log.Information(" --   Press Ctrl+C to stop   --");
	Log.Information(" ------------------------------");
	await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (TaskCanceledException)
{
	Log.Information("Cancellation handled.");
}
catch (Exception ex)
{
	Log.Fatal(ex, "There was a fatal error.");
	throw;
}
finally
{
	if (discordListener is not null)
	{
		await discordListener.StopAsync();
	}

	Log.Debug("Services disposing asynchronously.");
	await services.DisposeAsync();
	Log.Debug("Services disposed.");

	Log.Information("Shutdown complete.");
	Log.CloseAndFlush();
}

public static class Globals
{
	public const ulong DiscordServerId = 1057034458152304790;
}