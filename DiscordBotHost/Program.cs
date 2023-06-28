
using Azure.Storage.Blobs;

using DiscordBotHost.Commands.LinksChannel;
using DiscordBotHost.EntityFramework;
using DiscordBotHost.Features.Auditions.Parsers;
using DiscordBotHost.Storage;

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
	.AddLogging(builder => builder.AddSerilog())
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
	.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<Program>())
	.AddSingleton(_ => new QueueServiceClient(config["AZURE_STORAGE"]))
	.AddSingleton(_ => new BlobServiceClient(config["AZURE_STORAGE"]))
	.AddSingleton<AndroidShareQueueListener>(sp =>
		new AndroidShareQueueListener(
			queueClient: sp.GetRequiredService<QueueServiceClient>().GetQueueClient(config["AZURE_STORAGE_QUEUENAME"]),
			serviceScopeFactory: sp.GetRequiredService<IServiceScopeFactory>()))
	.AddSingleton<BlobStorage>(sp => new BlobStorage(sp.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(config["AZURE_STORAGE_CONTAINERNAME"])))
	.AddSingleton<GetTextFromUrl>()
	.AddDbContext<DiscordBotDbContext>(options =>
	{
		options.UseSqlServer(
			config["MSSQL_CONNECTIONSTRING"],
			sqlOptions => sqlOptions.EnableRetryOnFailure())
		.EnableSensitiveDataLogging();
	})
	.BuildServiceProvider();

if (EF.IsDesignTime)
{
	Log.Information("EF DesignTime, exiting host.");
	await services.DisposeAsync();
	return;
}

using (var scope = services.CreateScope())
{
	Log.Information("Migration of database started.");
	await scope.ServiceProvider.GetRequiredService<DiscordBotDbContext>().Database.MigrateAsync();
	Log.Information("Migration finished.");
}

// Services that require cleanup.
DiscordEventListener? discordListener = default;
AndroidShareQueueListener? storageQueueListener = default;

// Shutdown monitor
using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
	Log.Information("Ctrl+C pressed, shutting down.");
	shutdown.Cancel();
	e.Cancel = true;
};

try
{
	// Start the Discord bot
	discordListener = services.GetRequiredService<DiscordEventListener>();
	await discordListener.StartAsync(config["DISCORD_TOKEN"] ??
		throw new InvalidOperationException("DISCORD_TOKEN is a required configuration."));

	// Message bus
	storageQueueListener = services.GetRequiredService<AndroidShareQueueListener>();
	await storageQueueListener.StartAsync(shutdown.Token);

	// Instructions
	Log.Information(" ------------------------------");
	Log.Information(" --   Press Ctrl+C to stop   --");
	Log.Information(" ------------------------------");
	await Task.Delay(Timeout.Infinite, shutdown.Token);
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

	if (storageQueueListener is not null)
	{
		await storageQueueListener.StopAsync(CancellationToken.None);
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
	public const ulong DefaultGuildId = DiscordServerId;

	public const ulong DefaultBotLogChannelId = 1057059014950780938;
	public const ulong DefaultChannelId = DefaultBotLogChannelId;
}
