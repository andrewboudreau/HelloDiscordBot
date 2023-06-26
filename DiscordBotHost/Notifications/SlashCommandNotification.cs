using DiscordBotHost.EntityFramework;
using DiscordBotHost.Features;

using MediatR;

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

				var user = await dbContext.GetOrCreateAsync(notification.Message.User);
				user.SetLinksChannelId(channel.Id);

				await dbContext.SaveChangesAsync(CancellationToken.None);
				await notification.Message.RespondAsync($"Your links channel is not set to <#{channel.Id}>", ephemeral: true);
			}
		}
	}
}