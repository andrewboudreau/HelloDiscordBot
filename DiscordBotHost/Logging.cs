using Serilog.Events;

namespace DiscordBotHost
{
	internal class Logging
	{
		internal static Task LogAsync(LogMessage message)
		{
			var severity = message.Severity switch
			{
				LogSeverity.Critical => LogEventLevel.Fatal,
				LogSeverity.Error => LogEventLevel.Error,
				LogSeverity.Warning => LogEventLevel.Warning,
				LogSeverity.Info => LogEventLevel.Information,
				LogSeverity.Verbose => LogEventLevel.Verbose,
				LogSeverity.Debug => LogEventLevel.Debug,
				_ => LogEventLevel.Information
			};

			Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);

			return Task.CompletedTask;
		}
	}
}
