using DiscordBotHost.Notifications;

using MediatR;

namespace DiscordBotHost;

public class DiscordEventListener(DiscordSocketClient client, IServiceScopeFactory serviceScopeFactory)
{
	private readonly DiscordSocketClient client = client;
	private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;
	private readonly CancellationTokenSource cts = new();

	public async Task StartAsync(string discordToken)
	{
		Log.Information("DiscordEventListener starting.");
		Log.Debug("DiscordEventListener SocketClient Login starting.");

		await client.LoginAsync(TokenType.Bot, discordToken);

		Log.Debug("DiscordEventListener SocketClient Login completed.");
		Log.Debug("DiscordEventListener SocketClient starting.");

		await client.StartAsync();

		Log.Debug("DiscordEventListener SocketClient started.");
		Log.Debug("DiscordEventListener binding event handlers starting.");

		client.Ready += OnReadyAsync;
		client.MessageReceived += OnMessageReceivedAsync;
		client.ReactionAdded += OnReactionAdded;
		client.InteractionCreated += OnInteractionCreated;

		Log.Debug("DiscordEventListener binding event handlers completed.");
		Log.Debug("DiscordEventListener starting completed.");
	}

	public async Task StopAsync()
	{
		Log.Information("DiscordEventListener stopping.");
		Log.Debug("DiscordEventListener cancellation token cancelling.");

		cts.Cancel();

		Log.Debug("DiscordEventListener cancellation token cancelled.");
		Log.Debug("DiscordEventListener SocketClient Logout starting.");

		await client.LogoutAsync();

		Log.Debug("DiscordEventListener SocketClient Logout completed.");
		Log.Debug("DiscordEventListener SocketClient stopping.");

		await client.StopAsync();

		Log.Debug("DiscordEventListener SocketClient stopped.");
		Log.Debug("DiscordEventListener removing event handlers starting.");

		client.Ready -= OnReadyAsync;
		client.MessageReceived -= OnMessageReceivedAsync;
		client.ReactionAdded -= OnReactionAdded;
		client.InteractionCreated -= OnInteractionCreated;

		Log.Debug("DiscordEventListener removing event handlers completed.");
		Log.Information("DiscordEventListener stopped.");
	}

	protected virtual async Task OnInteractionCreated(SocketInteraction arg)
	{
		try
		{
			if (arg is SocketSlashCommand command)
			{
				await serviceScopeFactory.PublishNotification(
					new SlashCommandNotification(command), cts);
			}

			if (arg is SocketMessageComponent component)
			{
				await serviceScopeFactory.PublishNotification(
					new MessageComponentNotification(component), cts);
			}
		}
		catch(Exception ex)
		{
			Log.Error(ex, "Error in OnInteractionCreated");
			throw;
		}
	}

	protected virtual async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
	{
		await serviceScopeFactory.PublishNotification(
			new ReactionReceivedNotification(cachedMessage, cachedChannel, reaction, client), cts);
	}

	protected virtual async Task OnMessageReceivedAsync(SocketMessage arg)
	{
		await serviceScopeFactory.PublishNotification(
			new MessageReceivedNotification(arg, client), cts);
	}

	protected virtual async Task OnReadyAsync()
	{
		await serviceScopeFactory.PublishNotification(
			new ReadyNotification(client), cts);

		Log.Information("Bot is ready.");
	}
}

public static class ServiceScopeFactoryExtensions
{
	public static async Task PublishNotification<T>(this IServiceScopeFactory factory, T notification, CancellationTokenSource cts)
		where T : INotification
	{
		await using var scope = factory.CreateAsyncScope();
		var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
		await mediator.Publish(notification, cts.Token);
	}
}