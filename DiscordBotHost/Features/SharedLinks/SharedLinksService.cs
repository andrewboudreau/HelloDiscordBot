using DiscordBotHost.EntityFramework;
using DiscordBotHost.Features;

using MediatR;

namespace DiscordBotHost.Commands.LinksChannel
{
	public class SharedLinksService :
		INotificationHandler<SharedLinkReceivedNotification>,
		INotificationHandler<SlashCommandNotification>,
		INotificationHandler<ReadyNotification>
	{
		private readonly DiscordBotDbContext dbContext;
		private readonly DiscordSocketClient discordClient;

		public SharedLinksService(DiscordBotDbContext dbContext, DiscordSocketClient discordClient)
		{
			this.dbContext = dbContext;
			this.discordClient = discordClient;
		}

		public async Task Handle(ReadyNotification notification, CancellationToken cancellationToken)
		{
			Log.Information("Creating guild commands for {ServiceName}", nameof(SharedLinksService));
			foreach (var commandProperties in SharedLinksCommandDefinitions.SetLinksChannelCommands())
			{
				await notification.Client.Rest.CreateGuildCommand(commandProperties, Globals.DiscordServerId);
			}
		}

		public async Task Handle(SharedLinkReceivedNotification notification, CancellationToken cancellationToken)
		{
			var user = dbContext.Users.First();
			if (await discordClient.GetChannelAsync(user.LinksChannelId) is not IMessageChannel targetChannel)
			{
				Log.Error("The target channel was null when attempting to share.");
				return;
			}

			await targetChannel.SendMessageAsync(notification.Message.ToString());
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
			var user = await dbContext.GetOrCreateUser(command.User);
			await command.RespondAsync($"User <@{command.User.Id}> has links channel set to <#{user.LinksChannelId}>.");
		}

		private async Task SetLinksChannel(SocketSlashCommand command)
		{
			if (command.HasInvalidOption<SocketGuildChannel>("channel", out var channel))
			{
				await command.RespondAsync("You didn't specify a channel.", ephemeral: true);
				return;
			}

			var user = await dbContext.GetOrCreateUser(command.User);
			user.SetLinksChannelId(channel.Id);
			await dbContext.SaveChangesAsync();

			await command.RespondAsync($"Your links channel is set to <#{channel.Id}>", ephemeral: true);
		}
	}
}
