using MediatR;

namespace DiscordBotHost;

public class ReadyNotification : INotification
{
    public static readonly ReadyNotification Default = new();

    private ReadyNotification()
    {
    }
}
