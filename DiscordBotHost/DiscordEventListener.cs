using DiscordBotHost.Notifications;

using MediatR;

namespace DiscordBotHost;

/// <summary>
/// Created with CHATGPT https://chat.openai.com/share/f40a8c86-664b-46a3-88bb-1289e34cebb7
/// </summary>
public class DiscordEventListener
{
	private readonly DiscordSocketClient client;
	private readonly IServiceScopeFactory serviceScopeFactory;
	private readonly CancellationTokenSource cts;

	public DiscordEventListener(DiscordSocketClient client, IServiceScopeFactory serviceScopeFactory)
	{
		this.client = client;
		this.serviceScopeFactory = serviceScopeFactory;
		cts = new CancellationTokenSource();
	}

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

		Log.Debug("DiscordEventListener removing event handlers completed.");
		Log.Information("DiscordEventListener stopped.");
	}

	protected virtual async Task OnInteractionCreated(SocketInteraction arg)
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