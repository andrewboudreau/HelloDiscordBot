using MediatR;

namespace DiscordBotHost
{
	public class SharedLinkReceivedHandler : INotificationHandler<SharedLinkReceivedNotification>
	{
#if DEBUG
		private static readonly ulong BotTesting = 1057059014950780938;
		private static readonly ulong SharedLinksChannel = BotTesting;
#else
		private static readonly ulong SharedLinks = 1119660959888318486;
		private static readonly ulong SharedLinksChannel = SharedLinks;
#endif

		private readonly DiscordSocketClient discordClient;

		public SharedLinkReceivedHandler(DiscordSocketClient discordClient)
		{
			this.discordClient = discordClient;
		}

		public async Task Handle(SharedLinkReceivedNotification notification, CancellationToken cancellationToken)
		{

			if (await discordClient.GetChannelAsync(SharedLinksChannel) is not IMessageChannel targetChannel)

			{
				Log.Error("The target channel was null when attempting to share.");
				return;
			}

			await targetChannel.SendMessageAsync(notification.Message.ToString());
		}
	}
}