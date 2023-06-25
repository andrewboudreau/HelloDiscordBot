using DiscordBotHost.EntityFramework;

using MediatR;

using Microsoft.EntityFrameworkCore;

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
		private readonly DiscordBotDbContext dbContext;

		public SlashCommandHandler(DiscordBotDbContext dbContext)
		{
			this.dbContext = dbContext;
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

				var user = await dbContext.Users.FirstOrDefaultAsync(x => x.DiscordId == notification.Message.User.Id, CancellationToken.None);
				if (user == null)
				{
					user = dbContext.Add(
						new DiscordUser(0,
							notification.Message.User.Username,
							notification.Message.User.Id,
							firebaseId: "",
							channel.Id)).Entity;
				}

				user.SetLinksChannelId(channel.Id);

				await dbContext.SaveChangesAsync(CancellationToken.None);
				await notification.Message.RespondAsync($"Your links channel is not set to <#{channel.Id}>", ephemeral: true);
			}

			if (notification.CommandName.ToLower() == "monitor-url")
			{
				if (notification.Message.Data.Options.FirstOrDefault(o => o.Name == "url")?.Value is not string url)
				{
					await notification.Message.RespondAsync("You didn't specify a url.", ephemeral: true);
					return;
				}

				var user = await dbContext.Users.FirstOrDefaultAsync(x => x.DiscordId == notification.Message.User.Id, CancellationToken.None);
				if (user == null)
				{
					user = dbContext.Add(
						new DiscordUser(0,
							notification.Message.User.Username,
							notification.Message.User.Id,
							firebaseId: "",
							Globals.DefaultChannelId)).Entity;
				}

				await dbContext.SaveChangesAsync(CancellationToken.None);
				await notification.Message.RespondAsync($"We submitted a url for review '{url}'", ephemeral: true);
			}
		}
	}
}