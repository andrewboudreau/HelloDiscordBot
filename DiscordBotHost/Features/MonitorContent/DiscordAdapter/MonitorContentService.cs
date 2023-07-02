using Azure.Storage.Blobs.Models;

using DiscordBotHost.EntityFramework;
using DiscordBotHost.Features;
using DiscordBotHost.Features.Auditions.Parsers;
using DiscordBotHost.Features.ContentMonitor;
using DiscordBotHost.Features.MonitorContent.Commands;
using DiscordBotHost.Features.MonitorContent.Events;
using DiscordBotHost.Notifications;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace DiscordBotHost.Commands.LinksChannel
{
	public class MonitorContentService :
		INotificationHandler<ReadyNotification>,
		INotificationHandler<SlashCommandNotification>,
		INotificationHandler<MessageComponentNotification>,
		INotificationHandler<ContentChangeDetected>
	{
		private readonly DiscordSocketClient client;
		private readonly DiscordBotDbContext dbContext;
		private readonly ISender sender;
		private readonly OpenAIClient openai;

		public MonitorContentService(
			DiscordSocketClient client,
			DiscordBotDbContext dbContext,
			ISender sender,
			OpenAIClient openai)
		{
			this.client = client;
			this.dbContext = dbContext;
			this.sender = sender;
			this.openai = openai;
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
			if (command.HasInvalidOption("requestid", out int requestId))
			{
				await command.RespondAsync("You didn't specify a monitor request id.", ephemeral: true);
				return;
			}

			var request = await dbContext.MonitorContentRequests
				.Where(x => x.DiscordUserId == command.User.Id)
				.FirstOrDefaultAsync(x => x.MonitorContentRequestId == requestId);

			if (request is null)
			{
				await command.RespondAsync($"Could not find monitor request for id {requestId}", ephemeral: true);
				return;
			}

			await command.DeferAsync();

			var response = await sender.Send(new PerformInspection(requestId));

			await command.FollowupAsync($"Monitor request for id `{requestId}` by user <@{command.User.Id}> was run, difference was {response}");
		}

		private async Task ListMonitors(SocketSlashCommand command)
		{
			var user = await dbContext.GetOrCreateUser(command.User);

			var components = new ComponentBuilder();

			foreach (var chunk in dbContext.MonitorContentRequests.Where(x => x.DiscordUserId == user.DiscordUserId).ToList().Chunk(4).Take(4))
			{
				var buttonRow = new ActionRowBuilder();
				foreach (var request in chunk)
				{
					buttonRow.WithButton(
						ButtonBuilder.CreatePrimaryButton($"Run {request.MonitorContentRequestId}", $"monitor-run-{request.MonitorContentRequestId}"));
				}

				components.AddRow(buttonRow);
			}

			var built = components.Build();
			await command.RespondAsync($"User <@{command.User.Id}> has called monitor list.", components: built);
		}

		private async Task CreateMonitorForUrl(SocketSlashCommand command)
		{
			if (command.HasInvalidOption("url", out Uri? url) || url is null)
			{
				await command.RespondAsync("You didn't specify a url.", ephemeral: true);
				return;
			}

			var selector = command.GetOptionValue("selector", defaultValue: "body");

			var request = dbContext.MonitorContentRequests.Add(
				MonitorContentRequest.TwiceDailyForAMonth(url, command.User.Id, selector)).Entity;

			await dbContext.SaveChangesAsync();

			await command.RespondAsync($"User <@{command.User.Id}> has created monitor request {request.MonitorContentRequestId} for '{request.Url}'.");
		}

		public async Task Handle(MessageComponentNotification notification, CancellationToken cancellationToken)
		{
			if (notification.Component.Data.CustomId.StartsWith("monitor-"))
			{
				if (notification.Component.Data.CustomId.Split("-")[1] == "run")
				{
					var requestId = int.Parse(notification.Component.Data.CustomId.Split("-")[2]);
					await notification.Component.RespondAsync($"Monitor request for id `{requestId}` by user <@{notification.Component.User.Id}> would be run now.");
				}

				await notification.Component.Channel.SendMessageAsync("Button was clicked!");
			}
		}

		public async Task Handle(ContentChangeDetected notification, CancellationToken cancellationToken)
		{
			if (await client.GetChannelAsync(Globals.DefaultBotLogChannelId) is not IMessageChannel targetChannel)
			{
				Log.Error("The target channel was null when attempting to publish content changes.");
				return;
			}

			await targetChannel.SendMessageAsync($"Content {notification.MonitorContentRequest.Url} has changed.");

			var auditions = new ParseTextContentForAuditionCall(openai);
			var jobs = await auditions.ParseForAuditionCalls(notification.NewContent);

			foreach (var job in jobs)
			{
				job.Url = PrepareUrl(job.Url, notification.MonitorContentRequest.Url);
				await targetChannel.SendMessageAsync(embed: BuildJobEmbed(job));
				await Task.Delay(1000, cancellationToken);
			}
		}

		public static Embed BuildJobEmbed(Job job)
		{
			var embedBuilder = new EmbedBuilder();
			embedBuilder.WithColor(Color.Blue);

			if (!string.IsNullOrEmpty(job.Name))
			{
				embedBuilder.WithTitle(job.Name);
			}

			if (!string.IsNullOrEmpty(job.Company))
			{
				embedBuilder.WithAuthor(job.Company);
			}

			if (!string.IsNullOrEmpty(job.Description))
			{
				embedBuilder.WithDescription(job.Description);
			}

			if (!string.IsNullOrEmpty(job.Url))
			{
				embedBuilder.WithUrl(job.Url);
			}

			embedBuilder
				.AddField("Audition Date", job.Date ?? "n/a");

			return embedBuilder.Build();
		}

		public static string PrepareUrl(string? path, Uri monitorUrl)
		{
			if (string.IsNullOrEmpty(path) || !path.ToLowerInvariant().StartsWith("http"))
			{
				return monitorUrl.ToString();
			}

			if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out var link))
			{
				return monitorUrl.ToString();
			}

			if (link.IsAbsoluteUri)
			{
				return link.ToString();
			}

			return new Uri(monitorUrl, link).ToString();
		}
	}
}
