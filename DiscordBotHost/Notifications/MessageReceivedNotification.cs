using Discord.WebSocket;

using MediatR;

namespace DiscordBotHost
{
	public class MessageReceivedNotification : INotification
	{
		public MessageReceivedNotification(SocketMessage message, DiscordSocketClient client)
		{
			ArgumentNullException.ThrowIfNull(message);
			ArgumentNullException.ThrowIfNull(client);

			Message = message;
			Client = client;
		}

		public SocketMessage Message { get; }
		public DiscordSocketClient Client { get; }
	}
}