using DiscordBotHost.Features.ContentMonitor;
using DiscordBotHost.SeedWork;

namespace DiscordBotHost.Features.MonitorContent.Events
{
	public class ContentChangeDetected(string newContent, MonitorContentRequest monitorContentRequest, ContentInspection contentInspection) : IDomainEvent
	{
		private readonly string newContent = newContent;
		private readonly MonitorContentRequest monitorContentRequest = monitorContentRequest;
		private readonly ContentInspection contentInspection = contentInspection;
		private readonly DateTimeOffset occurredAt = DateTimeOffset.Now;

		public DateTimeOffset OccurredAt => occurredAt;

		public MonitorContentRequest MonitorContentRequest => monitorContentRequest;

		public ContentInspection ContentInspection => contentInspection;

		public string NewContent => newContent;
	}
}
