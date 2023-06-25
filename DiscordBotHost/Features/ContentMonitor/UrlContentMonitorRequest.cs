namespace DiscordBotHost.Features.ContentMonitor
{
	/// <summary>
	/// Represents a request to monitor the content of a URL.
	/// </summary>
	public class UrlContentMonitorRequest
	{
		public UrlContentMonitorRequest()
		{
		}

		ContentMonitorSourceType sourceType;

		/// <summary>
		/// Gets or sets the identifier of the URL content monitor request.
		/// </summary>
		public int UrlContentMonitorRequestId { get; set; }

		/// <summary>
		/// Gets or sets the URL to monitor.
		/// </summary>
		public Uri Url { get; set; }

		/// <summary>
		/// Gets the CSS selectors to use when finding the content in the html.
		/// </summary>
		public string Selectors { get; set; }

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

		public ulong DiscordUserId { get; set; }

		/// <summary>
		/// Creates a new instance that monitors a URL once daily for the next week.
		/// </summary>
		/// <param name="url">The URL to monitor.</param>
		/// <param name="selectors">The CSS selectors to use. If none are provided, the entire body of the HTML will be used.</param>
		/// <returns>A new <see cref="UrlContentMonitorRequest"/> instance.</returns>
		public static UrlContentMonitorRequest DailyForTheNextWeek(string url, ulong discordUserId, params string[] selectors) => new()
		{
			Url = new(url),
			Selectors = selectors.Any() ? string.Join("||", selectors) : "body",
			CreatedAt = DateTimeOffset.UtcNow,
			Interval = TimeSpan.FromDays(1),
			RunUntil = DateTimeOffset.UtcNow.AddDays(7),
			DiscordUserId = discordUserId
		};

		/// <summary>
		/// Creates a new instance that monitors a URL every hour indefinitely.
		/// </summary>
		/// <param name="url">The URL to monitor.</param>
		/// <param name="selectors">The CSS selectors to use. If none are provided, the entire body of the HTML will be used.</param>
		/// <returns>A new <see cref="UrlContentMonitorRequest"/> instance.</returns>
		public static UrlContentMonitorRequest EveryHourForever(Uri url, params string[] selectors) => new UrlContentMonitorRequest()
		{
			Url = url,
			Selectors = selectors.Any() ? string.Join("||", selectors) : "body",
			CreatedAt = DateTimeOffset.UtcNow,
			Interval = TimeSpan.FromHours(1),
			RunUntil = DateTimeOffset.MaxValue
		};
	}

	public enum ContentMonitorSourceType
	{
		Passive,
		Active
	}
}
