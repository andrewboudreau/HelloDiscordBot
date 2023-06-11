using Azure.Storage.Queues;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Hosting;

using Serilog;

namespace DiscordBotHost
{
	public class AndroidShareQueueListener : BackgroundService
	{
		private static readonly ulong channelToDumpShares = 1057059014950780938;
		private readonly QueueClient queueClient;
		private readonly DiscordSocketClient discordClient;

		public AndroidShareQueueListener(QueueClient queueClient, DiscordSocketClient discordClient)
		{
			this.queueClient = queueClient;
			this.discordClient = discordClient;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Log.Information("AndroidShareQueueListener starting.");

			if (await queueClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken) is not null)
				Log.Information("AndroidShareQueueListener queue created.");

			Log.Debug("AndroidShareQueueListener started.");

			while (!stoppingToken.IsCancellationRequested)
			{
				// Fetch up to 32 messages in one call
				var messages = await queueClient.ReceiveMessagesAsync(32, cancellationToken: CancellationToken.None);
				foreach (var message in messages.Value)
				{
					// Process the message
					if (await discordClient.GetChannelAsync(channelToDumpShares) is not IMessageChannel targetChannel)
					{
						Log.Error("The target channel was null when attempting to share.");
						return;
					}

					await targetChannel.SendMessageAsync(message.Body.ToString());
					await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, CancellationToken.None);
				}

				// Consider adding a delay here if the queue is often empty to avoid busy waiting
				await Task.Delay(CustomDelayProvider.GetDelay(), stoppingToken);
			}
		}
	}

	public static class CustomDelayProvider
	{
		private static readonly TimeSpan TenSeconds = TimeSpan.FromSeconds(2.5);
		private static readonly TimeSpan ThirtyMinutes = TimeSpan.FromMinutes(5);

		public static TimeSpan GetDelay()
		{
			var currentTime = DateTime.Now;
			bool isOffHours = currentTime.Hour >= 2 && currentTime.Hour < 7;
			return isOffHours ? ThirtyMinutes : TenSeconds;
		}
	}

	public static class DiscordSocketClientExtensions
	{
		public static async ValueTask<IMessageChannel> GetChannelAsync(this DiscordSocketClient client, ulong channelId)
		{
			var channel = client.GetChannel(channelId) as IMessageChannel;

			while (channel == null && client.ConnectionState != ConnectionState.Connected)
			{
				await Task.Delay(TimeSpan.FromSeconds(5));
				channel = client.GetChannel(channelId) as IMessageChannel;
			}

			if (channel == null)
			{
				throw new Exception($"Failed to get channel {channelId}.");
			}

			return channel;
		}
	}
}

