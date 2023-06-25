using DiscordBotHost.Features.Auditions.Parsers;

namespace Tests
{
	[TestClass]
	public class ParseUrlContentTests
	{
		private ParseHtmlElementContentsFromUrl parser;

		[TestInitialize]
		public void TestInitialize()
		{
			parser = new ParseHtmlElementContentsFromUrl();
		}

		[TestMethod]
		public async Task ReadContent()
		{
			var url = "https://www.theatreinchicago.com/auditions";
			var selector = ".post.post-list-item";

			var percentChange = await parser.DetectChange(url, selector, Serilog.Log.Information);
			percentChange = await parser.DetectChange(url, selector, Serilog.Log.Information);
		}

		[TestMethod]
		public async Task ReadOtherContent()
		{
			var url = "https://www.florentineopera.org/auditions-employment";
			var selector = ".Main-content";

			var percentChange = await parser.DetectChange(url, selector, Serilog.Log.Information);
			percentChange = await parser.DetectChange(url, selector, Serilog.Log.Information);
		}
	}
}