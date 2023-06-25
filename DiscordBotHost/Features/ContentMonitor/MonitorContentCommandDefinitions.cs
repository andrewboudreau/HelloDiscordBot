namespace DiscordBotHost.Commands.LinksChannel
{
	public static class MonitorContentCommandDefinitions
	{
		public static IEnumerable<SlashCommandProperties> MonitorContentCommands()
		{
			yield return new SlashCommandBuilder()
				.WithName("monitor-url")
				.WithDescription("Request a url to be monitored.")
				.AddOption(
					name: "Url",
					type: ApplicationCommandOptionType.String,
					description: "The url to monitor.",
					isRequired: true)
				.Build();

			yield return new SlashCommandBuilder()
				.WithName("monitor-list")
				.WithDescription("Lists the monitoring requests")
				.Build();

			yield return new SlashCommandBuilder()
				.WithName("monitor-run")
				.WithDescription("Runs a check for content change on the monitor request id.")
				.AddOption(
					name: "MonitorId",
					type: ApplicationCommandOptionType.Integer,
					description: "The monitor request id to run.",
					isRequired: true)
				.Build();
		}
	}
}
