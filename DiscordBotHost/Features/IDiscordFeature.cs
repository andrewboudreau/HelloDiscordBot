using Discord.Rest;

namespace DiscordBotHost.Features
{
	public interface IDiscordFeature
	{
		string FeaturePrefix { get; }

		Task InstallSlashCommands(DiscordSocketClient client);

		Task UninstallSlashCommands(DiscordSocketClient client);

		Task RegisterDiscordHandlers(DiscordSocketClient client);

	}
}
