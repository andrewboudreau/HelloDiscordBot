using DiscordBotHost.EntityFramework;
using DiscordBotHost.Features.ContentMonitor;
using DiscordBotHost.Features.MonitorContent.Commands;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

using System.Threading;

namespace DiscordBotHost.Features.MonitorContent
{
	public class ContentMonitoringScheduler(IServiceScopeFactory scopes) : BackgroundService
	{
		private readonly IServiceScopeFactory scopes = scopes;

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				using var scope = scopes.CreateScope();
				var dbContext = scope.ServiceProvider.GetRequiredService<DiscordBotDbContext>();
				var sender = scope.ServiceProvider.GetRequiredService<ISender>();

				if (await GetNextMonitorContentRequest(dbContext) is MonitorContentRequest request)
				{
					try
					{
						await sender.Send(new PerformInspection(request.MonitorContentRequestId), stoppingToken);
					}
					catch (Exception ex)
					{
						Log.Error(ex, "MonitorContentRequest {id} failed during content inspection.", request.MonitorContentRequestId);
					}
				}

				await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
			}
		}

		private static async Task<MonitorContentRequest?> GetNextMonitorContentRequest(DiscordBotDbContext dbContext)
		{
			var now = DateTimeOffset.UtcNow;

			var candidates = await dbContext.MonitorContentRequests
				.Include(mcr => mcr.ContentInspections)
				.OrderBy(mcr => mcr.CreatedAt)
				.Where(mcr => mcr.RunUntil > now && (mcr.Repeat == 0 || mcr.ContentInspections.Count < mcr.Repeat))
				.ToListAsync(CancellationToken.None);

			// Get last inspection for each active request
			var lastInspections = await dbContext.ContentInspections
				.Where(i => candidates.Select(r => r.MonitorContentRequestId).Contains(i.MonitorContentRequest.MonitorContentRequestId))
				.GroupBy(i => i.MonitorContentRequest.MonitorContentRequestId)
				.Select(g => g.OrderByDescending(i => i.CreatedAt).First())
				.ToListAsync(CancellationToken.None);

			// Find one request that needs to be run
			foreach (var request in candidates)
			{
				var lastInspection = lastInspections.FirstOrDefault(i => i.MonitorContentRequest.MonitorContentRequestId == request.MonitorContentRequestId);
				var lastRun = lastInspection?.CreatedAt ?? request.CreatedAt;

				if (lastRun + request.Interval <= now)
				{
					return request;
				}
			}

			return default;
		}
	}
}
