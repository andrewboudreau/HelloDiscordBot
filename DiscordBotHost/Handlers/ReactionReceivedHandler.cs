using Discord;
using Discord.WebSocket;

using MediatR;

using Serilog;

namespace DiscordBotHost
{
	public class ReactionReceivedHandler : INotificationHandler<ReactionReceivedNotification>
	{
		private static readonly ulong channelToSaveImagesTo = 1112051792721747988;
		private static readonly HttpClient httpClient = new();

        public ReactionReceivedHandler()
        {
			
        }

        public async Task Handle(ReactionReceivedNotification notification, CancellationToken cancellationToken)
		{
			var reaction = notification.Reaction;
			var cachedMessage = notification.Message;
			var cachedChannel = notification.Channel;
			var client = notification.Client;

			var task = reaction.Emote.Name switch
			{
				"💾" => SaveImage(reaction, cachedMessage, cachedChannel, client, cancellationToken),

				_ => Task.CompletedTask
			};

			await task;
		}

		private async Task SaveImage(SocketReaction reaction, Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, DiscordSocketClient client, CancellationToken cancellationToken)
		{
			// Get the message and check if it has an attachment
			var message = await cachedMessage.GetOrDownloadAsync();
			if (message.Attachments.Count > 0)
			{
				// Get the target channel
				if (client.GetChannel(channelToSaveImagesTo) is not IMessageChannel targetChannel)
				{
					Log.Error("The target channel was null when attempting to save.");
					return;
				}

				if (cachedChannel.Value is SocketGuildChannel guildChannel)
				{
					string infoMessage;
					string messageLink = $"https://discord.com/channels/{guildChannel.Guild.Id}/{cachedChannel.Id}/{cachedMessage.Id}";

					// Send the attachment to the target channel
					foreach (var attachment in message.Attachments)
					{
						if (message.Author.Id == reaction.UserId)
						{
							infoMessage = $"Source: {messageLink} | By: <@{message.Author.Id}>";
						}
						else
						{
							infoMessage = $"Source: {messageLink} | Created by: <@{message.Author.Id}> | Saved by: <@{reaction.UserId}>";
						}

						var stream = await httpClient.GetStreamAsync(attachment.Url, cancellationToken);
						await targetChannel.SendFileAsync(stream, attachment.Filename, infoMessage);
					}
				}
			}
		}
	}
}