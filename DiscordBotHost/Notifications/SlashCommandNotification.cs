using DiscordBotHost.EntityFramework;

using MediatR;

using Microsoft.Extensions.Options;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DiscordBotHost
{
	public class SlashCommandNotification : INotification
	{
		public SlashCommandNotification(SocketSlashCommand message)
		{
			Message = message;
		}

		public SocketSlashCommand Message { get; set; }
		public string CommandName => Message.Data.Name;
	}

	public class SlashCommandHandler : INotificationHandler<SlashCommandNotification>
	{
#if DEBUG
		private static readonly ulong BotTesting = 1057059014950780938;
		private static readonly ulong SharedLinksChannel = BotTesting;
#else
		private static readonly ulong SharedLinks = 1119660959888318486;
		private static readonly ulong SharedLinksChannel = SharedLinks;
#endif

		private readonly DiscordSocketClient discordClient;
		private readonly DiscordBotDbContext db;

		public SlashCommandHandler(DiscordSocketClient discordClient, DiscordBotDbContext db)
		{
			this.discordClient = discordClient;
			this.db = db;
		}

		public async Task Handle(SlashCommandNotification notification, CancellationToken cancellationToken)
		{
			if (notification.CommandName.ToLower() == "setlinkschannel")
			{
				if (notification.Message.Data.Options.FirstOrDefault(o => o.Name == "channel")?.Value is not SocketGuildChannel channel)
				{
					await notification.Message.RespondAsync("You didn't specify a channel.", ephemeral: true);
					return;
				}

				//if(db.Users.Any(x => x.DiscordId ==  ))
				db.Add(
					new DiscordUser(0,
						notification.Message.User.Username,
						notification.Message.User.Id,
						FirebaseId: "",
						channel.Id));
				db.SaveChanges();
			}

			if (await discordClient.GetChannelAsync(SharedLinksChannel) is not IMessageChannel targetChannel)
			{

				Log.Warning("GOT IT : " + notification.CommandName);
				return;
			}

			//await notification.Message.RespondAsync("GOT YOU with " + notification.CommandName);
		}
	}
}