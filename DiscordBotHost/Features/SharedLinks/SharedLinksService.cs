using DiscordBotHost.EntityFramework;
using DiscordBotHost.Features;

using MediatR;

namespace DiscordBotHost.Commands.LinksChannel
{
	public class SharedLinksService :
		INotificationHandler<SlashCommandNotification>,
		INotificationHandler<ReadyNotification>
	{
		private readonly DiscordBotDbContext dbContext;
		private User user;

		public SharedLinksService(DiscordBotDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		public async Task Handle(ReadyNotification notification, CancellationToken cancellationToken)
		{
			Log.Information("Creating guild commands for SharedLinks");
			foreach (var commandProperties in SharedLinksCommandDefinitions.SetLinksChannelCommands())
			{
				await notification.Client.Rest.CreateGuildCommand(commandProperties, Globals.DiscordServerId);
			}
		}

		public async Task Handle(SlashCommandNotification notification, CancellationToken cancellationToken)
		{
			var command = notification.Command;
			switch (command.Data.Name)
			{
				case SharedLinksCommandDefinitions.SetLinksChannel:
					await SetLinksChannel(command);
					break;

				case SharedLinksCommandDefinitions.ListLinksChannel:
					await ListLinksChannel(command);
					break;
			}
		}

		private async Task ListLinksChannel(SocketSlashCommand command)
		{
			user = await dbContext.GetOrCreateAsync(command.User);
			await command.RespondAsync($"User <@{command.User.Id}> has links channel set to <#{user.LinksChannelId}>.");
		}

		private async Task SetLinksChannel(SocketSlashCommand command)
		{
			if (command.HasInvalidOption<SocketGuildChannel>("channel", out var channel))
			{
				await command.RespondAsync("You didn't specify a channel.", ephemeral: true);
				return;
			}

			user = await dbContext.GetOrCreateAsync(command.User);
			user.SetLinksChannelId(channel.Id);
			await dbContext.SaveChangesAsync();

			await command.RespondAsync($"Your links channel is set to <#{channel.Id}>", ephemeral: true);
		}
	}
}
