using DiscordBotHost.Commands.LinksChannel;

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
		client.ReactionAdded += HandleReactionAdded;
		client.InteractionCreated += HandleInteractionCreated;

		Log.Debug("DiscordEventListener binding event handlers completed.");
		Log.Debug("DiscordEventListener starting completed.");
	}

	private async Task HandleInteractionCreated(SocketInteraction arg)
	{
		if (arg is SocketSlashCommand command) // Make sure it's a slash command
		{
			await using var scope = serviceScopeFactory.CreateAsyncScope();
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
			await mediator.Publish(new SlashCommandNotification(command), cts.Token);
		}
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
		client.ReactionAdded -= HandleReactionAdded;
		Log.Debug("DiscordEventListener removing event handlers completed.");

		Log.Information("DiscordEventListener stopped.");
	}

	private async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
	{
		await using var scope = serviceScopeFactory.CreateAsyncScope();
		var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
		await mediator.Publish(new ReactionReceivedNotification(cachedMessage, cachedChannel, reaction, client), cts.Token);
	}

	private async Task OnMessageReceivedAsync(SocketMessage arg)
	{
		await using var scope = serviceScopeFactory.CreateAsyncScope();
		var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
		await mediator.Publish(new MessageReceivedNotification(arg, client), cts.Token);
	}

	private async Task OnReadyAsync()
	{
		Log.Information("Bot is ready.");

		Log.Debug("DiscordEventListener SlashCommands registering.");

		foreach (var commandProperties in SharedLinksCommandDefinitions.SetLinksChannelCommands())
		{
			// https://github.com/discord-net/Discord.Net/issues/2221
			await client.Rest.CreateGuildCommand(commandProperties, Globals.DiscordServerId);
		}
		Log.Debug("DiscordEventListener SlashCommands registered.");

		await using var scope = serviceScopeFactory.CreateAsyncScope();
		var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
		await mediator.Publish(ReadyNotification.Default, cts.Token);
	}
}