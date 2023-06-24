using DiscordBotHost.EntityFramework;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

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

			//this.client.InteractionCreated += HandleCommandAsync;
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

		private async Task HandleListLinksChannelCommandAsync(SocketSlashCommand command)
		{
			var user = await dbContext.Users.FirstOrDefaultAsync(x => x.DiscordId == command.User.Id, CancellationToken.None);

			if (user is null)
			{
				await command.RespondAsync($"User {command.User.Id} was not found.");
				return;
			}

			await command.RespondAsync($"User <@{command.User.Id}> has links channel set to <#{user.LinksChannelId}>.");
		}

		public async Task HandleSetLinksChannelCommandAsync(SocketSlashCommand command)
		{
			if (command.Data.Options.FirstOrDefault(o => o.Name == "channel")?.Value is not SocketGuildChannel channel)
			{
				await command.RespondAsync("You didn't specify a channel.", ephemeral: true);
				return;
			}

			var user = await dbContext.Users.FirstOrDefaultAsync(x => x.DiscordId == command.User.Id, CancellationToken.None);
			if (user == null)
			{
				user = dbContext.Add(
					new DiscordUser(0,
						command.User.Username,
						command.User.Id,
						firebaseId: "",
						channel.Id)).Entity;
			}

			user.SetLinksChannelId(channel.Id);

			await dbContext.SaveChangesAsync(CancellationToken.None);
			await command.RespondAsync($"Your links channel is set to <#{channel.Id}>", ephemeral: true);
		}
	}
}
