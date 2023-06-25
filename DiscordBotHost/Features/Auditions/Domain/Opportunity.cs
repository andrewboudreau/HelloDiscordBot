using DiscordBotHost.Features.Auditions.DomainEvents;
using DiscordBotHost.SeedWork;

namespace DiscordBotHost.Features.Auditions
{
	public class Opportunity : AggregateRoot
	{
		private Opportunity()
		{
			Company = "";
			Type = "";
			ShowName = "";
			JobName = "";
			Description = "";
			Summary = "";
		}

		public int OpportunityId { get; private set; }

		public string Company { get; private set; }

		public string Type { get; private set; }

		public string ShowName { get; private set; }

		public string JobName { get; private set; }

		public string Description { get; private set; }

		public string Summary { get; private set; }

		public DateTimeOffset? AuditionDateTime { get; private set; }

		public DateTimeOffset? AuditionEndDateTime { get; private set; }

		public static Opportunity Create(
			string company,
			string type,
			string showName,
			string jobName,
			string description,
			string summary,
			DateTimeOffset? auditionDateTime,
			DateTimeOffset? endDateTime)
		{
			var opportunity =
				new Opportunity()
				{
					Company = company,
					Type = type,
					ShowName = showName,
					JobName = jobName,
					Description = description,
					Summary = summary,
					AuditionDateTime = auditionDateTime,
					AuditionEndDateTime = endDateTime
				};

			var created = new NewOpportunity(opportunity);
			opportunity.Add(created);

			return opportunity;
		}


/*
* 
{
  "type": "object",
  "properties": {
    "Company": {
      "type": "string",
      "description": "Name of the theater or production company"
    },
    "Type": {
      "type": "string",
      "description": "Type of the opportunity",
      "enum": ["Actor", "Actress", "Director", "Producer", "Writer", "Singer", "Ensemble", "Other"]
    },
    "ShowName": {
      "type": "string",
      "description": "Name of the show or performance"
    },
    "JobName": {
      "type": "string",
      "description": "Name of the job or role"
    },
    "Description": {
      "type": "string",
      "description": "Short description of the opportunity"
    },
    "Summary": {
      "type": "string",
      "description": "Full summary of the role"
    },
    "AuditionDateTime": {
      "type": "string",
      "format": "date-time",
      "description": "Audition start date and time in ISO 8601 format"
    },
    "AuditionEndDateTime": {
      "type": "string",
      "format": "date-time",
      "description": "Audition end date and time in ISO 8601 format"
    }
  },
  "required": ["Company", "Type", "ShowName", "JobName", "Description", "Summary", "AuditionDateTime"]
}
*/
	}
}
