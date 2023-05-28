using Discord;
using Discord.WebSocket;

using MediatR;

namespace DiscordBotHost
{
	public class ReactionReceivedNotification : INotification
	{
		public ReactionReceivedNotification(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction, DiscordSocketClient client)
		{

			ArgumentNullException.ThrowIfNull(message);
			ArgumentNullException.ThrowIfNull(channel);
			ArgumentNullException.ThrowIfNull(reaction);
			ArgumentNullException.ThrowIfNull(client);

			Message = message;
			Channel = channel;
			Reaction = reaction;
			Client = client;
		}

		public Cacheable<IUserMessage, ulong> Message { get; }
		public Cacheable<IMessageChannel, ulong> Channel { get; }
		public SocketReaction Reaction { get; }
		public DiscordSocketClient Client { get; }
	}
}