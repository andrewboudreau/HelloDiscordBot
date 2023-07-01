using DiscordBotHost.Features.ContentMonitor;
using DiscordBotHost.Storage;

namespace DiscordBotHost.Features.Auditions.Parsers
{
	public static class BlobStorageExtensions
	{
		public static async Task<string> Read(this BlobStorage store, ContentInspection inspection)
		{
			string previous = "";
			await store.Read(
				BlobStorage.PathForTextContent($"monitor-{inspection.MonitorContentRequest.MonitorContentRequestId}-inspection", inspection.ContentInspectionId),
				binaryData => previous = binaryData.ToString());

			return previous;
		}

		public static async Task<string> Save(this BlobStorage store, ContentInspection inspection, string content)
		{
			var path = BlobStorage.PathForTextContent($"monitor-{inspection.MonitorContentRequest.MonitorContentRequestId}-inspection", inspection.ContentInspectionId);
			await store.Save(path, BinaryData.FromString(content));
			return path;
		}

		public static Uri GetUrlForRead(this BlobStorage store, ContentInspection inspection)
		{
			var path = BlobStorage.PathForTextContent($"monitor-{inspection.MonitorContentRequest.MonitorContentRequestId}-inspection", inspection.ContentInspectionId);
			return store.GetReadUrl(path, TimeSpan.FromDays(700));
		}
	}
}