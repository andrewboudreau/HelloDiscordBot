using Discord;

using DiscordBotHost.EntityFramework;
using DiscordBotHost.Features;

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
		private readonly DiscordBotDbContext dbContext;

		public SharedLinkReceivedHandler(DiscordSocketClient discordClient, DiscordBotDbContext dbContext)
		{
			this.discordClient = discordClient;
			this.dbContext = dbContext;
		}

		public async Task Handle(SharedLinkReceivedNotification notification, CancellationToken cancellationToken)
		{
			var andrew = dbContext.Users.Where(x => x.Id == 1).Single();
			
			if (await discordClient.GetChannelAsync(andrew.LinksChannelId) is not IMessageChannel targetChannel)
			{
				Log.Error("The target channel was null when attempting to share.");
				return;
			}

			await targetChannel.SendMessageAsync(notification.Message.ToString());
		}
	}
}