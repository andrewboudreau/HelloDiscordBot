using MediatR;

namespace DiscordBotHost;

public class ReadyNotification : INotification
{
	public ReadyNotification(DiscordSocketClient client)
	{
		this.Client = client;
	}

	public DiscordSocketClient Client { get; }
}
