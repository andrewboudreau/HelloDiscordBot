using System;

public static class CustomDelayProvider
{
	private static readonly TimeSpan TenSeconds = TimeSpan.FromSeconds(10);
	private static readonly TimeSpan ThirtyMinutes = TimeSpan.FromMinutes(30);

	public static TimeSpan GetDelay()
	{
		var currentTime = DateTime.UtcNow; // Use DateTime.Now if you want local time instead of UTC
		bool isOffHours = currentTime.Hour >= 3 && currentTime.Hour < 7;
		return isOffHours ? ThirtyMinutes : TenSeconds;
	}
}


