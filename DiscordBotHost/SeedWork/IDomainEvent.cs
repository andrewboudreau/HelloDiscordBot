using MediatR;

namespace DiscordBotHost.SeedWork
{
	public interface IDomainEvent : INotification
	{
		DateTimeOffset OccurredAt { get; }
	}
}
