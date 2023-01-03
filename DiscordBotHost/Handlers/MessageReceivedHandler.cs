using Discord;

using MediatR;

namespace DiscordBotHost
{
    public class MessageReceivedHandler : INotificationHandler<MessageReceivedNotification>
    {
        public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"MediatR works! (Received a message by {notification.Message.Author.Username})");

            if (notification.Message.Content == "ping")
                await notification.Message.Channel.SendMessageAsync($"pong to {notification.Message.Author.Username}");

            else if (notification.Message.Content == "test")
                await notification.Message.Channel.SendMessageAsync($"Yeah {notification.Message.Author.Username}, I get it! You're testing!");

            else if (notification.Message.Content.StartsWith("echo"))
                await notification.Message.Channel.SendMessageAsync(notification.Message.Content[4..]);
        }
    }
}