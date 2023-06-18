using Microsoft.EntityFrameworkCore;

namespace DiscordBotHost.EntityFramework
{
	public class DiscordBotDbContext : DbContext
	{
		public const string ConnectionStringKeyName = "MSSQL_CONNECTIONSTRING";

		public DiscordBotDbContext(DbContextOptions<DiscordBotDbContext> options)
			: base(options)
		{
			Users = default!;
		}

		public DbSet<DiscordUser> Users { get; set; }
	}

	public record DiscordUser(int Id, string Name, ulong DiscordId, string FirebaseId, ulong LinksChannelId)
	{
		public static DiscordUser Empty { get; } 
			= new DiscordUser(0, "", 0, "", 1057059014950780938);

		public static DiscordUser Create(string name, ulong discordId, ulong linksChannel)
			=> new(0, name, discordId, "", linksChannel);
	}
}
