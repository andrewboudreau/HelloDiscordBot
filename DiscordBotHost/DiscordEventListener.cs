using Discord.WebSocket;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace DiscordBotHost;

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

        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(SocketMessage arg)
    {
        var requestAborted = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var scope = serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Publish(new MessageReceivedNotification(arg), requestAborted.Token);        
    }

    private async Task OnReadyAsync()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Publish(ReadyNotification.Default, cancellationToken);
    }
}