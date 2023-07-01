using DiscordBotHost.Features.ContentMonitor;

namespace Tests
{
	[TestClass]
	public class ParseUrlContentTests
	{
		private ContentChangeDetector parser;

		[TestInitialize]
		public void TestInitialize()
		{
			parser = new ContentChangeDetector();
		}

		[TestMethod]
		public void ReadContent()
		{
			var url = "https://www.theatreinchicago.com/auditions";
			var selector = ".post.post-list-item";

			var percentChange = parser.DetectDifferences(url, selector, Serilog.Log.Information);
			percentChange = parser.DetectDifferences(url, selector, Serilog.Log.Information);
		}

		[TestMethod]
		public void ReadOtherContent()
		{
			var url = "https://www.florentineopera.org/auditions-employment";
			var selector = ".Main-content";

			var percentChange = parser.DetectDifferences(url, selector, Serilog.Log.Information);
			percentChange = parser.DetectDifferences(url, selector, Serilog.Log.Information);
		}
	}
}