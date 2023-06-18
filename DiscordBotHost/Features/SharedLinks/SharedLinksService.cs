using DiscordBotHost.EntityFramework;

namespace DiscordBotHost.Commands.LinksChannel
{
	public class SharedLinksService
	{
		private DiscordSocketClient client;
		private DiscordBotDbContext dbContext;

		public SharedLinksService(DiscordSocketClient client, DiscordBotDbContext dbContext)
		{
			this.client = client;
			this.dbContext = dbContext;

			this.client.InteractionCreated += HandleCommandAsync;
		}

		private async Task HandleCommandAsync(SocketInteraction arg)
		{
			// Make sure it's a slash command
			if (arg is not SocketSlashCommand command)
				return;

			switch (command.Data.Name)
			{
				case "SetLinksChannel":
					await HandleSetLinksChannelCommandAsync(command);
					break;
				case "ListLinksChannel":
					await HandleListLinksChannelCommandAsync(command);
					break;
			}
		}

		private async Task HandleSetLinksChannelCommandAsync(SocketSlashCommand command)
		{
			// Get the channel
			var channelOption = command.Data.Options.FirstOrDefault(o => o.Name == "channel");
			if (channelOption == null)
			{
				await command.RespondAsync("You didn't specify a channel.", ephemeral: true);
				return;
			}

			var channel = channelOption.Value as SocketGuildChannel;

			// Save it to your repository here
			// ... use _dbContext to access your database

			// Let the user know it worked
			await command.RespondAsync($"Successfully set {channel.Name} as the links channel.");
		}

		private async Task HandleListLinksChannelCommandAsync(SocketSlashCommand command)
		{
			// Retrieve the links channel from your repository here
			// ... use _dbContext to access your database

			// Let the user know which channel is the links channel
			// ... your response logic goes here
		}

		// Your SetLinksChannelCommands method goes here
	}
}
