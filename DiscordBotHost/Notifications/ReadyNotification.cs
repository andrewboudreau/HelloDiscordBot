using DiscordBotHost.Notifications;

namespace DiscordBotHost;

public class ReadyNotification : IDiscordNotification
{
	public ReadyNotification(DiscordSocketClient client)
	{
		this.Client = client;
	}

	public DiscordSocketClient Client { get; }
}
