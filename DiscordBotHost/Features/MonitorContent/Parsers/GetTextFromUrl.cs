using Fizzler.Systems.HtmlAgilityPack;

using HtmlAgilityPack;

using System.Text;

using Log = Serilog.Log;

namespace DiscordBotHost.Features.Auditions.Parsers
{
	public class GetTextFromUrl
	{
		private static readonly HashSet<string> blockElements = new() { "div", "p", "h1", "h2", "h3", "h4", "h5", "h6", "li", "ul", "ol", "pre", "address", "blockquote", "dl", "fieldset", "form", "hr", "noscript", "table" };
		private static readonly HashSet<string> ignoredElements = new() { "noscript", "hr", "script", "style", "meta", "link", "head", "iframe" };

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
				TraverseNode(node, stringBuilder);
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

		private void TraverseNode(HtmlNode node, StringBuilder stringBuilder)
		{
			if (node.NodeType == HtmlNodeType.Element)
			{
				if (ignoredElements.Contains(node.Name))
				{
					return;
				}

				if (node.Name == "a")
				{
					var innerText = node.InnerText.Trim();
					if (!string.IsNullOrEmpty(innerText)) // Check if the inner text is not empty
					{
						stringBuilder.Append($"<a href=\"{node.GetAttributeValue("href", "")}\">{innerText}</a>");
					}
				}
				else
				{
					foreach (var child in node.ChildNodes)
					{
						TraverseNode(child, stringBuilder);
					}
					if (blockElements.Contains(node.Name) && !EndsWithTwoWhitespaceThings(stringBuilder))
					{
						stringBuilder.AppendLine();
					}
					else if (!EndsWithTwoWhitespaceThings(stringBuilder))
					{
						stringBuilder.Append(' ');
					}
				}
			}
			else if (node.NodeType == HtmlNodeType.Text)
			{
				var trimmedText = node.InnerText
					.Replace("&nbsp;", " ")
					.Replace("  ", " ")
					.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine)
					.Trim();

				if (trimmedText != "")
				{
					stringBuilder.Append(trimmedText.Trim());
				}
			}
		}

		private static bool EndsWithTwoWhitespaceThings(StringBuilder stringBuilder)
		{
			string content = stringBuilder.ToString();
			if(content.Length == 0)
			{
				return true;
			}

			if (content.Length < 2)
			{
				return false;
			}

			// Check for double newlines
			if (content.Length >= 5 &&
				content.EndsWith(Environment.NewLine + Environment.NewLine + Environment.NewLine))
			{
				return true;
			}

			// Check for double spaces followed by a newline
			if (content.Length >= 3 &&
				content.EndsWith("  " + Environment.NewLine))
			{
				return true;
			}

			return false;
		}

		public static Uri TrimUri(Uri uri)
		{
			string scheme = uri.Scheme.ToLowerInvariant();
			string host = uri.DnsSafeHost.ToLowerInvariant();
			string path = uri.AbsolutePath.TrimEnd('/');

			return new Uri($"{scheme}://{host}{path}");
		}

		public static Uri TrimUri(string uri)
			=> TrimUri(new Uri(uri));
	}
}