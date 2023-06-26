namespace DiscordBotHost.Features.ContentMonitor.Parsers
{
	public struct NormalizedValue
	{
		private double value;

		public NormalizedValue(double value)
		{
			if (value > 1)
			{
				// If the value is above 1, treat it as a percentage.
				value /= 100.0;
			}

			this.value = value < 0 ? 0 : value > 1 ? 1 : value;
		}

		public static implicit operator NormalizedValue(double value) => new NormalizedValue(value);
		public static implicit operator NormalizedValue(float value) => new NormalizedValue(value);
		public static implicit operator NormalizedValue(int value) => new NormalizedValue(value);

		public static implicit operator double(NormalizedValue nv) => nv.value;
		public static implicit operator float(NormalizedValue nv) => (float)nv.value;
		public static implicit operator int(NormalizedValue nv) => (int)nv.value;

		public override readonly string ToString() => value.ToString();
	}

	public static class NormalizedValueExtensions
	{
		/// <summary>
		/// Returns the normalized value as a percentage.
		/// </summary>
		/// <param name="normalizedValue">The normalized value to convert to a percentage.</param>
		/// <returns>The value of the normalized value as a percentage.</returns>
		public static double AsPercentage(this NormalizedValue normalizedValue)
		{
			return ((double)normalizedValue) * 100;
		}
	}
}
