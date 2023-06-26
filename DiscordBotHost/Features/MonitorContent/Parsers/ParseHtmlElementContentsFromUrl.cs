using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

using Fizzler.Systems.HtmlAgilityPack;

using HtmlAgilityPack;

using Log = Serilog.Log;

namespace DiscordBotHost.Features.Auditions.Parsers
{
	public class ParseHtmlElementContentsFromUrl
	{
		private const char lineEnding = '\n';

		private static readonly HttpClient httpClient = new();

		public static async Task<string> TransformHtmlToStringContent(string url, string selector)
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

			if (nodes is null || !nodes.Any())
			{
				return string.Empty;
			}

			return string.Join(lineEnding, nodes.Select(node => node.InnerText));
		}

		public async Task<double> DetectChange(string previousContent, string content, Action<string> renderer)
		{
			var diffBuilder = new InlineDiffBuilder(new Differ());
			var diffResult = diffBuilder.BuildDiffModel(previousContent, content);

			int oldLines = previousContent.Split(lineEnding).Count();
			int newLines = content.Split(lineEnding).Count();

			int addedLines = diffResult.Lines.Where(x => x.Type == ChangeType.Inserted).Count();
			int deletedLines = diffResult.Lines.Where(x => x.Type == ChangeType.Deleted).Count();

			double changePercentage = (double)(addedLines + deletedLines) / (oldLines + newLines) * 100;

			renderer($"Change detected. {addedLines} lines added, {deletedLines} lines deleted. Overall change: {changePercentage}%");

			// Print diff
			foreach (var line in diffResult.Lines)
			{
				if (line.Type == ChangeType.Inserted)
				{
					renderer($"+ {line.Text}");
				}
				else if (line.Type == ChangeType.Deleted)
				{
					renderer($"- {line.Text}");
				}
				else if (line.Type == ChangeType.Unchanged)
				{
					renderer($"  {line.Text}");
				}
				else if (line.Type == ChangeType.Imaginary)
				{
					renderer($"? {line.Text}");
				}
			}

			previousContent = content;
			return changePercentage;
		}

		private static async Task<string> GetHtml(string url)
		{
			Log.Information("Getting html from {url}", url);
			using var response = await httpClient.GetAsync(url);

			if (!response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				throw new InvalidOperationException($"There was an error reading '{url}'. Error {response.StatusCode} - {content}.");
			}

			return await response.Content.ReadAsStringAsync();
		}
	}
}