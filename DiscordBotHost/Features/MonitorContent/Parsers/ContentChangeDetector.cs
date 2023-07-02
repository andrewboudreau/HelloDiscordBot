using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace DiscordBotHost.Features.ContentMonitor
{
	public class ContentChangeDetector
	{
		private static readonly string lineEnding = Environment.NewLine;

		public double DetectDifferences(string previousContent, string content, Action<string> diffs, Action<string> added)
		{
			var diffBuilder = new InlineDiffBuilder(new Differ());
			var diffResult = diffBuilder.BuildDiffModel(previousContent, content);

			int oldLines = previousContent.Split(lineEnding).Length;
			int newLines = content.Split(lineEnding).Length;

			int addedLines = diffResult.Lines.Where(x => x.Type == ChangeType.Inserted).Count();
			int deletedLines = diffResult.Lines.Where(x => x.Type == ChangeType.Deleted).Count();

			double changePercentage = (double)(addedLines + deletedLines) / (oldLines + newLines);

			diffs($"Change detected. {addedLines} lines added, {deletedLines} lines deleted. Overall change: {changePercentage * 100}%");

			// Print diff
			foreach (var line in diffResult.Lines)
			{
				if (line.Type == ChangeType.Inserted)
				{
					diffs($"+ {line.Text}");
					added(line.Text);
				}
				else if (line.Type == ChangeType.Deleted)
				{
					diffs($"- {line.Text}");
				}
				
				else if (line.Type == ChangeType.Imaginary)
				{
					diffs($"? {line.Text}");
				}
			}

			previousContent = content;
			return changePercentage;
		}
	}
}
