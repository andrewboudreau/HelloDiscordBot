using MediatR;

namespace DiscordBotHost
{
	public class SlashCommandNotification : INotification
	{
		public SlashCommandNotification(SocketSlashCommand command)
		{
			Command = command;
		}

		public SocketSlashCommand Command { get; set; }

		public string CommandName => Command.Data.Name;
	}
}