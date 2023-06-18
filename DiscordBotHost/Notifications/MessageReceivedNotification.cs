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

	public class MessageReceivedHandler : INotificationHandler<MessageReceivedNotification>
	{
		private readonly Func<Guid> factory;

		public MessageReceivedHandler(Func<Guid> factory)
		{
			this.factory = factory;
		}

		public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
		{
			var task = notification.Message.Content.Split(' ')[0] switch
			{
				"ping" => notification.Message.Channel.SendMessageAsync($"pong to {notification.Message.Author.Username}"),
				"test" => notification.Message.Channel.SendMessageAsync($"Yeah {notification.Message.Author.Username}, I get it! You're testing!"),
				"echo" => notification.Message.Channel.SendMessageAsync(notification.Message.Content[4..]),
				"guid" => notification.Message.Channel.SendMessageAsync($"Your GUID is \r\n`{factory()}`\r\n`{factory()}`"),
				_ => Task.CompletedTask
			};

			await task;
		}
	}
}