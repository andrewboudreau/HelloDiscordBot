using MediatR;

namespace DiscordBotHost
{
	public class SharedLinkReceivedNotification : INotification
	{
		public SharedLinkReceivedNotification(string message)
		{
			ArgumentNullException.ThrowIfNull(message);

			Message = message;
		}

		public string Message { get; }
	}
}