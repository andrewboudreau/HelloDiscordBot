namespace DiscordBotHost.Commands.LinksChannel
{
	public static class OptionDataExtensions
	{
		public static bool TryGetOptionValue<T>(this SocketSlashCommand command, string name, out T value, T defaultValue = default!)
		{
			value = defaultValue;
			var option = command.Data.Options?.FirstOrDefault(o => o.Name.ToLowerInvariant() == name.ToLowerInvariant());

			if (option == null)
			{
				return false;
			}

			if (option.Value is T typedValue)
			{
				value = typedValue;
				return true;
			}

			if (option.Value is string stringValue && int.TryParse(stringValue, out var intValue))
			{
				value = (T)(object)intValue;
				return true;
			}

			if (option.Value is string stringUrlValue && Uri.TryCreate(stringUrlValue, UriKind.Absolute, out var uriValue))
			{
				value = (T)(object)uriValue;
				return true;
			}

			if (option.Value is int intValue2)
			{
				value = (T)(object)intValue2;
				return true;
			}

			if (option.Value is long longValue)
			{
				value = (T)(object)(int)longValue;
				return true;
			}

			return false;
		}

		public static bool HasInvalidOption<T>(this SocketSlashCommand command, string name, out T value, T defaultValue = default!)
			=> !TryGetOptionValue(command, name, out value, defaultValue);

		public static T GetOptionValue<T>(this SocketSlashCommand command, string name, T defaultValue = default!)
			=> TryGetOptionValue(command, name, out var value, defaultValue) ? value : defaultValue;
	}
}
