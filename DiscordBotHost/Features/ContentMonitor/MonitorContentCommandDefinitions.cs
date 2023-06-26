namespace DiscordBotHost.Commands.LinksChannel
{
	public static class MonitorContentCommandDefinitions
	{
		public const string MonitorUrl = "monitorurl";
		public const string MonitorList = "monitorlist";
		public const string MonitorRun = "monitor";

		public static IEnumerable<SlashCommandProperties> MonitorContentCommands()
		{
			yield return new SlashCommandBuilder()
				.WithName(MonitorUrl)
				.WithDescription("Request a url to be monitored.")
				.AddOption(
					name: "url",
					type: ApplicationCommandOptionType.String,
					description: "The url to monitor.",
					isRequired: true)
				.AddOption(
					name: "selector",
					type: ApplicationCommandOptionType.String,
					description: "A css selector from which to scope the contents to monitor.",
					isRequired: false)
				.Build();

			yield return new SlashCommandBuilder()
				.WithName(MonitorList)
				.WithDescription("Lists the monitoring requests ids")
				.Build();

			yield return new SlashCommandBuilder()
				.WithName(MonitorRun)
				.WithDescription("Runs a check for content change on the monitor request id.")
				.AddOption(
					name: "requestid",
					type: ApplicationCommandOptionType.Integer,
					description: "The monitor request id to run.",
					isRequired: true)
				.Build();
		}
	}
}
