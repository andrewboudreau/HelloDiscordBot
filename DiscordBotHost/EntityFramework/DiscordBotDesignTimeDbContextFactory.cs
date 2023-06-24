using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DiscordBotHost.EntityFramework
{
	public class DiscordBotDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DiscordBotDbContext>
	{
		public DiscordBotDbContext CreateDbContext(string[] args)
		{
			Log.Information("DesignTimeDbContext loading for {dbContext}.", nameof(DiscordBotDbContext));

			var connectionString = args.FirstOrDefault(IsConnectionString) 
				?? ParseFromConfigurations();

			Log.Information("Found connection string '{connectionString}'", connectionString);

			var optionsBuilder = new DbContextOptionsBuilder<DiscordBotDbContext>();
			optionsBuilder.UseSqlServer(connectionString);
			return new DiscordBotDbContext(optionsBuilder.Options);
		}

		private static string ParseFromConfigurations()
		{
			IConfiguration config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", false, true)
				.AddJsonFile("appsettings.secret.json", true, true)
				.AddEnvironmentVariables()
				.Build();

			return config[DiscordBotDbContext.ConnectionStringKeyName]
				?? throw new InvalidOperationException($"Connection string is null for configuration value '{DiscordBotDbContext.ConnectionStringKeyName}'");
		}

		private static bool IsConnectionString(string str)
		{
			if (str.Contains("data", StringComparison.OrdinalIgnoreCase) ||
				str.Contains("source", StringComparison.OrdinalIgnoreCase) ||
				str.Contains("server", StringComparison.OrdinalIgnoreCase) ||
				str.Contains("database", StringComparison.OrdinalIgnoreCase) ||
				str.Contains("initial", StringComparison.OrdinalIgnoreCase) ||
				str.Contains("catalog", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}
	}
}
