using Fizzler.Systems.HtmlAgilityPack;

using HtmlAgilityPack;

using System.Globalization;
using System.Text;

using Log = Serilog.Log;

namespace DiscordBotHost.Features.Auditions.Parsers
{
	public class GetTextFromUrl
	{
		private static readonly HttpClient httpClient;

		static GetTextFromUrl()
		{
			httpClient = new HttpClient();
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

			StringBuilder stringBuilder = new();
			foreach (var node in nodes)
			{
				foreach (var line in node.InnerText.Split("\n"))
				{
					var nodeText = line
						.Replace("\r", "")
						.Replace("&nbsp;", " ", true, CultureInfo.InvariantCulture)
						.Replace("  ", " ")
						.Trim();

					if (nodeText.Trim() == "")
					{
						continue;
					}

					stringBuilder.AppendLine(nodeText.Trim());
				}

				if (node != nodes.Last())
				{
					stringBuilder.AppendLine();
				}
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
	}
}