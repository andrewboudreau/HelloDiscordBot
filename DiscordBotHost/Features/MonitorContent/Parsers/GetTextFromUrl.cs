using Fizzler.Systems.HtmlAgilityPack;

using HtmlAgilityPack;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Log = Serilog.Log;

namespace DiscordBotHost.Features.Auditions.Parsers
{
	public partial class GetTextFromUrl
	{
		private const string lineEnding = "\r\n";

		private static readonly HttpClient httpClient = new();
		static GetTextFromUrl()
		{
			httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
			httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36 Edg/115.0.0.0");
		}

		public async Task<string> TransformHtmlToStringContent(Uri url, string selector)
		{
			var html = await GetHtml(url);

			var document = new HtmlDocument();
			document.LoadHtml(html);

			IEnumerable<HtmlNode>? nodes;
			if (selector.Contains("//*"))
			{
				nodes = document.DocumentNode.SelectNodes(selector);
			}
			else
			{
				nodes = document.DocumentNode.QuerySelectorAll(selector);
			}

			if (nodes == null || !nodes.Any())
			{
				return string.Empty;
			}

			var stringBuilder = new StringBuilder();
			var repeatedNewLineRegex = RepeatedNewLines();

			foreach (var node in nodes)
			{
				var nodeText = node.InnerText.Trim()
					.Replace("&nbsp;", " ", true, CultureInfo.InvariantCulture)
					.Replace("\r\n", "\n")
					.Replace("  ", " ")
					.Trim();

				nodeText = repeatedNewLineRegex.Replace(nodeText, "\n");
				if (nodeText.Trim() == "")
				{
					continue;
				}

				stringBuilder.AppendLine(nodeText.Trim());
			}

			return stringBuilder.ToString();
		}


		public static async Task<string> GetHtml(Uri url)
		{
			Log.Information("Getting html from {url}", url);
			using var response = await httpClient.GetAsync(url);

			if (!response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				var ex = new InvalidOperationException($"There was an error reading '{url}'. Error {response.StatusCode} - {content}.");
				Log.Error(ex, "Error getting url content.");
				throw ex;
			}

			return await response.Content.ReadAsStringAsync();
		}

		[GeneratedRegex("\n+")]
		private static partial Regex RepeatedNewLines();
	}
}