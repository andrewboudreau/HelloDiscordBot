using DiscordBotHost.EntityFramework;
using DiscordBotHost.Features;
using DiscordBotHost.Features.ContentMonitor;

using MediatR;

namespace DiscordBotHost.Commands.LinksChannel
{
    public class MonitorContentService :
		INotificationHandler<ReadyNotification>,
		INotificationHandler<SlashCommandNotification>
	{
		private readonly DiscordBotDbContext dbContext;
		private User user;

		public MonitorContentService(DiscordBotDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		public async Task Handle(ReadyNotification notification, CancellationToken cancellationToken)
		{
			Log.Information("Creating guild commands for MonitorContent");
			foreach (var commandProperties in MonitorContentCommandDefinitions.MonitorContentCommands())
			{
				await notification.Client.Rest.CreateGuildCommand(commandProperties, Globals.DiscordServerId);
			}
		}

		public async Task Handle(SlashCommandNotification notification, CancellationToken cancellationToken)
		{
			user = await dbContext.GetOrCreateAsync(notification.Command.User);

			Task result = notification.Command.Data.Name switch
			{
				MonitorContentCommandDefinitions.MonitorUrl
					=> CreateMonitorForUrl(notification.Command),

				MonitorContentCommandDefinitions.MonitorList
					=> ListMonitors(notification.Command),

				MonitorContentCommandDefinitions.MonitorRun
					=> RunMonitorRequest(notification.Command),

				_ => Task.CompletedTask
			};

			await result;
		}

		private async Task RunMonitorRequest(SocketSlashCommand command)
		{
			if (command.Data.Options.FirstOrDefault(o => o.Name == "requestid")?.Value is not int monitorRequestId)
			{
				await command.RespondAsync("You didn't specify a monitor request id.", ephemeral: true);
				return;
			}

			await command.RespondAsync($"Monitor request for id {monitorRequestId} by user {user.Id} would be run now.");
			//await dbContext.SaveChangesAsync(CancellationToken.None);
		}

		private async Task ListMonitors(SocketSlashCommand command)
		{
			foreach (var request in dbContext.ContentMonitorRequests.Where(x => x.DiscordUserId == user.DiscordUserId).ToList())
			{
				var items = string.Join("\n", $"{request.UrlContentMonitorRequestId}: {request.Url}");
			}

			await command.RespondAsync($"User <@{command.User.Id}> has called monitor list.");
		}

		private async Task CreateMonitorForUrl(SocketSlashCommand command)
		{
			if (!command.TryGetValue("url", out string url))
			{
				await command.RespondAsync("You didn't specify a url.", ephemeral: true);
				return;
			}

			if (!command.TryGetValue("selector", out string selector))
			{
				selector = "body";
			}

			dbContext.ContentMonitorRequests.Add(
				UrlContentMonitorRequest.DailyForTheNextWeek(url, command.User.Id, selector));

			await dbContext.SaveChangesAsync();

			await command.RespondAsync($"User <@{command.User.Id}> has links channel set to <#{user.LinksChannelId}>.");
		}
	}
}
