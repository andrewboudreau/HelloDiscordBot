using DiscordBotHost.Features.ContentMonitor;
using DiscordBotHost.SeedWork;

namespace DiscordBotHost.Features.MonitorContent.Events
{
	public class ContentChangeDetected : IDomainEvent
	{
		private readonly DateTimeOffset occurredAt;
		private readonly MonitorContentRequest monitorContentRequest;
		private readonly ContentInspection contentInspection;

		public ContentChangeDetected(MonitorContentRequest monitorContentRequest, ContentInspection contentInspection)
		{
			this.occurredAt = DateTimeOffset.Now;
			this.monitorContentRequest = monitorContentRequest;
			this.contentInspection = contentInspection;
		}

		public DateTimeOffset OccurredAt => occurredAt;

		public MonitorContentRequest MonitorContentRequest => monitorContentRequest;

		public ContentInspection ContentInspection => contentInspection;
	}
}
