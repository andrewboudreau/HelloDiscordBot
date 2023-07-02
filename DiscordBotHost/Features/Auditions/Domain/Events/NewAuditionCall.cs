using DiscordBotHost.SeedWork;

namespace DiscordBotHost.Features.Auditions.DomainEvents
{
    public class NewAuditionCall : IDomainEvent
    {
        public NewAuditionCall(AuditionCall audition)
        {
			Audition = audition;
            OccurredAt = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset OccurredAt { get; init; }

        public AuditionCall Audition { get; init; }
    }
}
