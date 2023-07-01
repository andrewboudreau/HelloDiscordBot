using System.Collections.ObjectModel;

namespace DiscordBotHost.Features.ContentMonitor
{
	/// <summary>
	/// Represents a request to monitor the content of a URL.
	/// </summary>
	public class MonitorContentRequest
	{
		private MonitorContentRequest() { }

		ContentMonitorSourceType sourceType;

		private Collection<ContentInspection> contentInspections;

		/// <summary>
		/// Gets or sets the identifier of the URL content monitor request.
		/// </summary>
		public int MonitorContentRequestId { get; set; }

		/// <summary>
		/// Gets or sets the URL to monitor.
		/// </summary>
		public Uri Url { get; set; }

		/// <summary>
		/// Gets the CSS selectors to use when finding the content in the html.
		/// </summary>
		public string Selector { get; set; }

		/// <summary>
		/// Gets the amount of time to wait between each check.
		/// </summary>
		public TimeSpan Interval { get; set; }

		/// <summary>
		/// Gets the number of times to repeat the check, default means check only once.
		/// </summary>
		public int Repeat { get; set; }

		/// <summary>
		/// Gets the datetime until the monitor should run, default means run forever.
		/// </summary>
		public DateTimeOffset RunUntil { get; set; } = DateTimeOffset.MaxValue;

		/// <summary>
		/// Gets or sets a value which indicates at what percentage of difference between the old and new content should the monitor trigger.
		/// </summary>
		/// <remarks>Defaults to 0.1.</remarks>
		public double DifferenceThreshold { get; set; } = 0.1;

		/// <summary>
		/// Gets the datetime when the request was created.
		/// </summary>
		public DateTimeOffset CreatedAt { get; private set; }

		/// <summary>
		/// Gets or sets the value of the user who created the request.
		/// </summary>
		public ulong DiscordUserId { get; set; }

		/// <summary>
		/// Gets the content inspections for this monitor.
		/// </summary>
		public IReadOnlyCollection<ContentInspection> ContentInspections => contentInspections;

		/// <summary>
		/// Starts a new inspection for this request.
		/// </summary>
		/// <returns>The content inspection.</returns>
		public ContentInspection StartNewInspection() => ContentInspection.From(this);

		/// <summary>
		/// Creates a new instance that monitors a URL once daily for the next week.
		/// </summary>
		/// <param name="url">The URL to monitor.</param>
		/// <param name="selectors">The CSS selectors to use. If none are provided, the entire body of the HTML will be used.</param>
		/// <returns>A new <see cref="MonitorContentRequest"/> instance.</returns>
		public static MonitorContentRequest TwiceDailyForAMonth(Uri url, ulong discordUserId, params string[] selectors) => new()
		{
			Url = url,
			Selector = selectors.Length != 0 ? string.Join("||", selectors) : "body",
			CreatedAt = DateTimeOffset.UtcNow,
			Interval = TimeSpan.FromHours(12),
			RunUntil = DateTimeOffset.UtcNow.AddMonths(1),
			DiscordUserId = discordUserId
		};

		/// <summary>
		/// Creates a new instance that monitors a URL every hour indefinitely.
		/// </summary>
		/// <param name="url">The URL to monitor.</param>
		/// <param name="selectors">The CSS selectors to use. If none are provided, the entire body of the HTML will be used.</param>
		/// <returns>A new <see cref="MonitorContentRequest"/> instance.</returns>
		public static MonitorContentRequest TwiceQuickly(Uri url, ulong discordUserId, params string[] selectors) => new()
		{
			Url = url,
			Selector = selectors.Any() ? string.Join("||", selectors) : "body",
			CreatedAt = DateTimeOffset.UtcNow,
			Interval = TimeSpan.FromSeconds(30),
			RunUntil = DateTimeOffset.UtcNow.AddSeconds(45),
			DiscordUserId = discordUserId
		};
	}

	public enum ContentMonitorSourceType
	{
		Passive,
		Active
	}
}
