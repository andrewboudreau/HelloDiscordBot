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

		public DbSet<DiscordUser> Users { get; set; }

		public DbSet<Opportunity> Opportunities { get; set; }

		public DbSet<UrlContentMonitorRequest> ContentMonitorRequests { get; set; }
		
		public DbSet<UrlContentInspection> ContentInspections { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Opportunity>().HasKey(x => x.OpportunityId);
			modelBuilder.Entity<UrlContentMonitorRequest>().HasKey(x => x.UrlContentMonitorRequestId);
			modelBuilder.Entity<UrlContentInspection>().HasKey(x => x.UrlContentInspectionId);
		}
	}
}
