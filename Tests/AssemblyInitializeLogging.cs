using Serilog;

namespace Tests
{
	[TestClass]
	public class AssemblyInitializeLogging
	{
		[AssemblyInitialize]
		public static void AssemblyInit(TestContext _)
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console(outputTemplate: "{Message}{NewLine}", standardErrorFromLevel: null)
				.CreateLogger();
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			Log.Information("Closing logger");
			Log.CloseAndFlush();
		}
	}
}
