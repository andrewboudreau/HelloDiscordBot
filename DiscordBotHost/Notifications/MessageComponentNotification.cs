using MediatR;

namespace DiscordBotHost.Notifications
{
	public class MessageComponentNotification : IDiscordNotification
	{
		public MessageComponentNotification(SocketMessageComponent component)
		{
			Component = component;
		}

		public SocketMessageComponent Component { get; private set; }
	}
}