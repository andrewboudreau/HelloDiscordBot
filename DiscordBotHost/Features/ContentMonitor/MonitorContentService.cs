using DiscordBotHost.EntityFramework;
using DiscordBotHost.Features.ContentMonitor;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace DiscordBotHost.Commands.LinksChannel
{
	public class MonitorContentService
	{
		private DiscordSocketClient client;
		private DiscordBotDbContext dbContext;

		public static async Task CreateGuildCommands(DiscordSocketClient client)
		{
			foreach (var commandProperties in MonitorContentCommandDefinitions.MonitorContentCommands())
			{
				await client.Rest.CreateGuildCommand(commandProperties, Globals.DiscordServerId);
			}
		}

		public MonitorContentService(DiscordSocketClient client, DiscordBotDbContext dbContext)
		{
			this.client = client;
			this.dbContext = dbContext;

			this.client.InteractionCreated += HandleCommandAsync;
			//this.client.Ready += HandleReadyAsync;
		}

		private Task HandleReadyAsync() 
			=> CreateGuildCommands(client);

		private async Task HandleCommandAsync(SocketInteraction arg)
		{
			// Make sure it's a slash command
			if (arg is not SocketSlashCommand command)
				return;

			switch (command.Data.Name)
			{
				case "monitor-url":
					await CreateMonitorForUrl(command);
					break;

				case "monitor-list":
					await ListMonitors(command);
					break;

				case "monitor-run":
					await RunMonitorRequest(command);
					break;
			}
		}

		private async Task RunMonitorRequest(SocketSlashCommand command)
		{
			if (command.Data.Options.FirstOrDefault(o => o.Name == "MonitorId")?.Value is not int monitorRequestId)
			{
				await command.RespondAsync("You didn't specify a monitor request id.", ephemeral: true);
				return;
			}

			var user = await dbContext.Users.FirstOrDefaultAsync(x => x.DiscordId == command.User.Id, CancellationToken.None);
			if (user is null)
			{
				await command.RespondAsync($"User {command.User.Id} was not found.");
				return;
			}

			await command.RespondAsync($"Monitor request for id {monitorRequestId} by user {command.User.Id} would be run now.");
			//await dbContext.SaveChangesAsync(CancellationToken.None);
		}

		private async Task ListMonitors(SocketSlashCommand command)
		{
			var user = await dbContext.Users.FirstOrDefaultAsync(x => x.DiscordId == command.User.Id, CancellationToken.None);
			if (user is null)
			{
				await command.RespondAsync($"Monitor-Request: User {command.User.Id} was not found.");
				return;
			}

			await command.RespondAsync($"User <@{command.User.Id}> has links channel set to <#{user.LinksChannelId}>.");
		}

		private async Task CreateMonitorForUrl(SocketSlashCommand command)
		{
			var user = await dbContext.Users.FirstOrDefaultAsync(x => x.DiscordId == command.User.Id, CancellationToken.None);
			if (user == null)
			{
				user = dbContext.Add(
					new DiscordUser(0,
						command.User.Username,
						command.User.Id,
						firebaseId: "",
						Globals.DefaultChannelId)).Entity;
			}

			var request = UrlContentMonitorRequest
				.DailyForTheNextWeek("https://www.florentineopera.org/auditions-employment", command.User.Id, ".Main-content");

			dbContext.ContentMonitorRequests.Add(request);
			await dbContext.SaveChangesAsync();

			await command.RespondAsync($"User <@{command.User.Id}> has links channel set to <#{user.LinksChannelId}>.");
		}
	}
}
