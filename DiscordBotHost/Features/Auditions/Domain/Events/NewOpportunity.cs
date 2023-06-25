using DiscordBotHost.SeedWork;

namespace DiscordBotHost.Features.Auditions.DomainEvents
{
    public class NewOpportunity : IDomainEvent
    {
        public NewOpportunity(Opportunity opportunity)
        {
            Opportunity = opportunity;
            OccurredAt = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset OccurredAt { get; init; }

        public Opportunity Opportunity { get; init; }
    }
}
