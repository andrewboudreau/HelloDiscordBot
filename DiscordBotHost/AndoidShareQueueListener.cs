using MediatR;

using Microsoft.Extensions.Hosting;

namespace DiscordBotHost
{
	public class AndroidShareQueueListener : BackgroundService
	{
		private readonly QueueClient queueClient;
		private readonly IServiceScopeFactory serviceScopeFactory;

		public AndroidShareQueueListener(QueueClient queueClient, IServiceScopeFactory serviceScopeFactory)
		{
			this.queueClient = queueClient;
			this.serviceScopeFactory = serviceScopeFactory;
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
					await using var scope = serviceScopeFactory.CreateAsyncScope();
					var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

					try
					{
						await mediator.Publish(new SharedLinkReceivedNotification(message.Body.ToString()), stoppingToken);
						await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, CancellationToken.None);
					}
					catch (Exception)
					{
						Log.Error("Failed to publish SharedLinkReceivedNotification internally.");
					}
				}

				// Consider adding a delay here if the queue is often empty to avoid busy waiting
				await Task.Delay(CustomDelayProvider.GetDelay(), stoppingToken);
			}
		}
	}

	public static class CustomDelayProvider
	{
		private static readonly TimeSpan TwoSeconds = TimeSpan.FromSeconds(2);
		private static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);

		public static TimeSpan GetDelay()
		{
			return TwoSeconds;
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

