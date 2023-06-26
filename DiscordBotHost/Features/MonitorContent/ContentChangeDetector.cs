﻿using DiffPlex.DiffBuilder.Model;
using DiffPlex.DiffBuilder;
using DiffPlex;
using DiscordBotHost.Features.ContentMonitor.Parsers;

namespace DiscordBotHost.Features.ContentMonitor
{
	public class ContentChangeDetector
	{
		private const char lineEnding = '\n';

		public double DetectDifferences(string previousContent, string content, Action<string> renderer)
		{
			var diffBuilder = new InlineDiffBuilder(new Differ());
			var diffResult = diffBuilder.BuildDiffModel(previousContent, content);

			int oldLines = previousContent.Split(lineEnding).Length;
			int newLines = content.Split(lineEnding).Length;

			int addedLines = diffResult.Lines.Where(x => x.Type == ChangeType.Inserted).Count();
			int deletedLines = diffResult.Lines.Where(x => x.Type == ChangeType.Deleted).Count();

			NormalizedValue changePercentage = (double)(addedLines + deletedLines) / (oldLines + newLines);

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
	}
}
