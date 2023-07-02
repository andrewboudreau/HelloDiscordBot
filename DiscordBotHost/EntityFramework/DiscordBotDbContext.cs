using DiscordBotHost.Features.Auditions;
using DiscordBotHost.Features.ContentMonitor;

using Microsoft.EntityFrameworkCore;

namespace DiscordBotHost.EntityFramework
{
	public class DiscordBotDbContext : DbContext
	{
		public const string ConnectionStringKeyName = "MSSQL_CONNECTIONSTRING";

#pragma warning disable CS8618 // required for EF.
		public DiscordBotDbContext(DbContextOptions<DiscordBotDbContext> options)
#pragma warning restore CS8618
			: base(options)
		{
		}

		public DbSet<User> Users { get; set; }

		public DbSet<AuditionCall> Opportunities { get; set; }

		public DbSet<MonitorContentRequest> MonitorContentRequests { get; set; }

		public DbSet<ContentInspection> ContentInspections { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>().HasKey(x => x.Id);
			modelBuilder.Entity<User>().Property(x => x.Id).ValueGeneratedNever();

			modelBuilder.Entity<AuditionCall>().HasKey(x => x.AuditionId);

			modelBuilder.Entity<MonitorContentRequest>().HasKey(x => x.MonitorContentRequestId);

			modelBuilder.Entity<MonitorContentRequest>().Property(x => x.Interval)
				.HasConversion(
					convertToProviderExpression: timespan => timespan.Ticks,
					convertFromProviderExpression: ticks => TimeSpan.FromTicks(ticks));

			modelBuilder.Entity<ContentInspection>().HasKey(x => x.ContentInspectionId);
		}
	}
}
