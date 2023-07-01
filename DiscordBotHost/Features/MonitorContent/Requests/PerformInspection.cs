using DiscordBotHost.EntityFramework;
using DiscordBotHost.Features.Auditions.Parsers;
using DiscordBotHost.Features.ContentMonitor;
using DiscordBotHost.Storage;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace DiscordBotHost.Features.MonitorContent.Commands
{
	public record PerformInspection(int MonitorContentRequestId) : IRequest<double>;

	public class PerformInspectionRequestHandler(DiscordBotDbContext dbContext, BlobStorage textBlobs, GetTextFromUrl getTextFromUrl, IPublisher publisher)
		: IRequestHandler<PerformInspection, double>
	{
		private readonly DiscordBotDbContext dbContext = dbContext;
		private readonly BlobStorage textBlobs = textBlobs;
		private readonly GetTextFromUrl getTextFromUrl = getTextFromUrl;
		private readonly IPublisher publisher = publisher;

		public async Task<double> Handle(PerformInspection request, CancellationToken cancellationToken)
		{
			var monitorContentRequest =
				await GetMonitorContentRequest(request.MonitorContentRequestId);

			string previousContent = string.Empty;
			if (await GetLastInspection(monitorContentRequest.MonitorContentRequestId) is ContentInspection lastInspection)
			{
				previousContent = (await textBlobs.Read(lastInspection)) ?? string.Empty;
			}

			var content = await getTextFromUrl.TransformHtmlToStringContent(monitorContentRequest.Url, monitorContentRequest.Selector);

			var inspection = monitorContentRequest.StartNewInspection();
			inspection.Compare(previousContent, content);

			dbContext.ContentInspections.Add(inspection);
			await dbContext.SaveChangesAsync(CancellationToken.None);

			inspection.SetContentSnapshotUrl(textBlobs.GetUrlForRead(inspection));
			await dbContext.SaveChangesAsync(CancellationToken.None);

			await textBlobs.Save(inspection, content);
			foreach (var domainEvent in inspection.IDomainEvents)
			{
				await publisher.Publish(domainEvent, CancellationToken.None);
			}

			return inspection.DifferenceValue;
		}

		private async Task<MonitorContentRequest> GetMonitorContentRequest(int requestId)
		{
			return await dbContext.MonitorContentRequests.FindAsync(new object[] { requestId }, cancellationToken: CancellationToken.None)
				?? throw new InvalidOperationException($"PerformInspection was null for '{requestId}'"); ;
		}

		private async Task<ContentInspection?> GetLastInspection(int requestId)
		{
			return await dbContext.ContentInspections
				.OrderByDescending(x => x.ContentInspectionId)
				.Where(x => x.MonitorContentRequest.MonitorContentRequestId == requestId)
				.FirstOrDefaultAsync();
		}
	}
}
