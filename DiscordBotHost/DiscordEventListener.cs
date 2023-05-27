using Discord;
using Discord.WebSocket;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

namespace DiscordBotHost;

/// <summary>
/// Created with CHATGPT https://chat.openai.com/share/f40a8c86-664b-46a3-88bb-1289e34cebb7
/// </summary>
public class DiscordEventListener
{
	private readonly CancellationToken cancellationToken;

	private readonly DiscordSocketClient client;
	private readonly IServiceScopeFactory serviceScopeFactory;

	public DiscordEventListener(DiscordSocketClient client, IServiceScopeFactory serviceScopeFactory)
	{
		this.client = client;
		this.serviceScopeFactory = serviceScopeFactory;
		cancellationToken = new CancellationTokenSource().Token;
	}

	public Task StartAsync()
	{
		client.Ready += OnReadyAsync;
		client.MessageReceived += OnMessageReceivedAsync;
		client.ReactionAdded += HandleReactionAdded;

		return Task.CompletedTask;
	}

	private static ulong ChannelToSaveImagesTo = 1112051792721747988; 
	private static readonly HttpClient httpClient = new HttpClient();

	private async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
	{
		// Check if the reaction is the disk emoji
		if (reaction.Emote.Name == "💾") // Replace "disk" with the actual name of your emoji
		{
			// Get the message and check if it has an attachment
			var message = await cachedMessage.GetOrDownloadAsync();
			if (message.Attachments.Count > 0)
			{
				// Get the target channel

				if (client.GetChannel(ChannelToSaveImagesTo) is not IMessageChannel targetChannel)
				{
					Log.Error("The target channel was null when attempting to save.");
					return;
				}

				// Send the attachment to the target channel
				foreach (var attachment in message.Attachments)
				{
					var stream = await httpClient.GetStreamAsync(attachment.Url);
					await targetChannel.SendFileAsync(stream, attachment.Filename);
				}

				// Send the link to the source message
				if (cachedChannel.Value is SocketGuildChannel guildChannel)
				{
					string messageLink = $"https://discord.com/channels/{guildChannel.Guild.Id}/{cachedChannel.Id}/{cachedMessage.Id}";
					string infoMessage = $"Source: {messageLink} | Created by: <@{message.Author.Id}> | Saved by: <@{reaction.UserId}>";
					await targetChannel.SendMessageAsync(infoMessage);
				}
			}
		}
	}

	private async Task OnMessageReceivedAsync(SocketMessage arg)
	{
		var requestAborted = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		using var scope = serviceScopeFactory.CreateScope();
		var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
		await mediator.Publish(new MessageReceivedNotification(arg), requestAborted.Token);
	}

	private async Task OnReadyAsync()
	{
		using var scope = serviceScopeFactory.CreateScope();
		var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
		await mediator.Publish(ReadyNotification.Default, cancellationToken);
	}
}