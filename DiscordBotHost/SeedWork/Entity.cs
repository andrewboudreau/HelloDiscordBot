namespace DiscordBotHost.SeedWork
{
	public abstract class AggregateRoot : Entity
	{

	}

	public abstract class Entity
	{
		private static readonly IReadOnlyCollection<IDomainEvent> empty = Array.Empty<IDomainEvent>().AsReadOnly();

		private List<IDomainEvent>? domainEvents;

		public IReadOnlyCollection<IDomainEvent> IDomainEvents => domainEvents?.AsReadOnly() ?? empty;

		protected void AddEvent(IDomainEvent eventItem)
		{
			domainEvents ??= new List<IDomainEvent>();
			domainEvents.Add(eventItem);
		}

		public void ClearIDomainEvents()
		{
			domainEvents?.Clear();
		}
	}
}
