using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenAI;

using Serilog;
using Serilog.Events;

namespace DiscordBotHost;

public class Bot
{
	private static IConfigurationRoot Configuration = default!;

	private static ServiceProvider ConfigureServices()
	{
		Configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", false, true)
			.AddJsonFile("appsettings.secret.json", true, true)
			.AddEnvironmentVariables()
			.Build();

		var openAiClient = new OpenAIClient(
			new OpenAIAuthentication(
				Configuration["OPENAI_API_KEY"], 
				Configuration["OPENAI_ORGANIZATION_ID"]));

		return new ServiceCollection()
			.AddSingleton<IConfiguration>(Configuration)
			.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<Bot>())
			.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = true,
				MessageCacheSize = 100,
				GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
				LogLevel = LogSeverity.Info
			}))
			.AddSingleton<DiscordEventListener>()
			.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
			.BuildServiceProvider();
	}

	public static async Task Main()
	{
		await new Bot().RunAsync();
	}

	private async Task RunAsync()
	{
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Verbose()
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.CreateLogger();

		await using var services = ConfigureServices();

		var client = services.GetRequiredService<DiscordSocketClient>();
		client.Log += LogAsync;

		var listener = services.GetRequiredService<DiscordEventListener>();
		await listener.StartAsync();

		await client.LoginAsync(TokenType.Bot, Configuration["DISCORD_TOKEN"]);
		await client.StartAsync();

		await Task.Delay(Timeout.Infinite);
	}

	private static Task LogAsync(LogMessage message)
	{
		var severity = message.Severity switch
		{
			LogSeverity.Critical => LogEventLevel.Fatal,
			LogSeverity.Error => LogEventLevel.Error,
			LogSeverity.Warning => LogEventLevel.Warning,
			LogSeverity.Info => LogEventLevel.Information,
			LogSeverity.Verbose => LogEventLevel.Verbose,
			LogSeverity.Debug => LogEventLevel.Debug,
			_ => LogEventLevel.Information
		};

		Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);

		return Task.CompletedTask;
	}
}