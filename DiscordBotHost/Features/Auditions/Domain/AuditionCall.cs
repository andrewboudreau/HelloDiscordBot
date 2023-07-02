using DiscordBotHost.Features.Auditions.DomainEvents;
using DiscordBotHost.SeedWork;

namespace DiscordBotHost.Features.Auditions
{
	public class AuditionCall : AggregateRoot
	{
		private AuditionCall()
		{
			Company = "";
			Name = "";
			Type = "";
			Description = "";
			Date = default;
			Url = default;
		}

		public int AuditionId { get; private set; }
		public string Company { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public string Description { get; set; }
		public string? Date { get; set; }
		public string? Url { get; set; }		

		public static AuditionCall Create(
			string company,
			string name,
			string type,
			string description,
			string? date, 
			string? url)
		{
			var audition =
				new AuditionCall()
				{
					Company = company,
					Name = name,
					Type = type,
					Description = description,
					Date = date,
					Url = url
				};

			var created = new NewAuditionCall(audition);
			audition.AddEvent(created);

			return audition;
		}
	}
}
