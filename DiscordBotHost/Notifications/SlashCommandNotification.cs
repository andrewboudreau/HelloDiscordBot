using DiscordBotHost.Notifications;

namespace DiscordBotHost
{
	public class SlashCommandNotification : IDiscordNotification
	{
		public SlashCommandNotification(SocketSlashCommand command)
		{
			Command = command;
		}

		public SocketSlashCommand Command { get; set; }

		public string CommandName => Command.Data.Name;
	}
}