using DiscordBotHost.Features.MonitorContent.Events;
using DiscordBotHost.SeedWork;

namespace DiscordBotHost.Features.ContentMonitor
{
	/// <summary>
	/// Manages the monitoring of content from a URL. This class represents a single iteration of getting the content 
	/// from the URL, extracting the text from the content, comparing the text, and storing the differences.
	/// </summary>
	public class ContentInspection : Entity
	{
		/// <summary>
		/// Delegate for comparing content.
		/// </summary>
		private readonly Func<string, string, (string[] Differences, double Difference)> compareContent;

		/// <summary>
		/// Private constructor used for initialization.
		/// </summary>
		private ContentInspection()
		{
			var detector = new ContentChangeDetector();
			compareContent = (prev, next) =>
			{
				var differences = new List<string>();
				var difference = detector.DetectDifferences(prev, next, diff => differences.Add(diff));
				return (differences.ToArray(), difference);
			};
		}

		/// <summary>
		/// Public constructor initializing the class with a specific comparison method.
		/// </summary>
		/// <param name="compareContent">A method to be used for content comparison.</param>
		public ContentInspection(Func<string, string, (string[] Differences, double Difference)> compareContent)
		{
			this.compareContent = compareContent;
		}

		/// <summary>
		/// Unique identifier for this inspection.
		/// </summary>
		public int ContentInspectionId { get; private set; }

		/// <summary>
		/// Associated monitor request for this inspection.
		/// </summary>
		public MonitorContentRequest MonitorContentRequest { get; protected set; }

		/// <summary>
		/// Gets the threshold value for the difference to be considered a change.
		/// </summary>
		public double DifferenceThreshold { get; private set; }

		/// <summary>
		/// Gets the value of the difference between the previous and next content.
		/// </summary>
		public double DifferenceValue { get; private set; }

		/// <summary>
		/// Stores differences as a string.
		/// </summary>
		public string Differences { get; private set; }

		/// <summary>
		/// Time when the inspection was created.
		/// </summary>
		public DateTimeOffset CreatedAt { get; private set; }

		/// <summary>
		/// Possible error during the content comparison, if not errors occurred, this will be null.
		/// </summary>
		/// <remarks>This is only non-null when there was an error.</remarks>
		public string? Error { get; private set; }

		/// <summary>
		/// Compare content using the delegate provided in the constructor.
		/// </summary>
		/// <param name="previous">Previous content string.</param>
		/// <param name="next">Next content string.</param>
		/// <exception cref="InvalidOperationException">Thrown when no compare strategy is set.</exception>
		public void Compare(string previous, string next)
			=> Compare(previous, next, compareContent);

		/// <summary>
		/// Compare content using a specified comparison method.
		/// </summary>
		/// <param name="previous">Previous content string.</param>
		/// <param name="next">Next content string.</param>
		/// <param name="compareContent">Specific content comparison method.</param>
		public void Compare(string previous, string next, Func<string, string, (string[] Differences, double Difference)> compareContent)
		{
			var result = compareContent(previous, next);

			DifferenceThreshold = MonitorContentRequest.DifferenceThreshold;
			DifferenceValue = result.Difference;
			Differences = string.Join('\n', result.Differences);

			if (DifferenceValue >= DifferenceThreshold)
			{
				Add(new ContentChangeDetected(MonitorContentRequest, this));
			}
		}

		/// <summary>
		/// Set an error message.
		/// </summary>
		/// <param name="error">The error message.</param>
		public void SetError(string error)
		{
			Error = error;
		}

		public static ContentInspection From(MonitorContentRequest request)
		{
			return new ContentInspection
			{
				MonitorContentRequest = request,
				DifferenceThreshold = request.DifferenceThreshold,
				CreatedAt = DateTimeOffset.UtcNow
			};
		}
	}
}
