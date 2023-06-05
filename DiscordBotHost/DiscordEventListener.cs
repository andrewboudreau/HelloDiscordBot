using Discord;
using Discord.WebSocket;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace DiscordBotHost;

/// <summary>
/// Created with CHATGPT https://chat.openai.com/share/f40a8c86-664b-46a3-88bb-1289e34cebb7
/// </summary>
public class DiscordEventListener
{
	private readonly CancellationToken cancellationToken;

	private readonly DiscordSocketClient client;
	private readonly IServiceScopeFactory serviceScopeFactory;

	public DiscordEventListener(DiscordSocketClient client, IServiceScopeFactory serviceScopeFactory)
	{
		this.client = client;
		this.serviceScopeFactory = serviceScopeFactory;
		cancellationToken = new CancellationTokenSource().Token;
	}

	public Task StartAsync()
	{
		client.Ready += OnReadyAsync;
		client.MessageReceived += OnMessageReceivedAsync;
		client.ReactionAdded += HandleReactionAdded;

		return Task.CompletedTask;
	}

	private async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
	{
		var requestAborted = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		using var scope = serviceScopeFactory.CreateScope();
		var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
		await mediator.Publish(new ReactionReceivedNotification(cachedMessage, cachedChannel, reaction, client), requestAborted.Token);
	}

	private async Task OnMessageReceivedAsync(SocketMessage arg)
	{
		var requestAborted = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		using var scope = serviceScopeFactory.CreateScope();
		var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
		await mediator.Publish(new MessageReceivedNotification(arg, client), requestAborted.Token);
	}

	private async Task OnReadyAsync()
	{
		using var scope = serviceScopeFactory.CreateScope();
		var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
		await mediator.Publish(ReadyNotification.Default, cancellationToken);
	}
}