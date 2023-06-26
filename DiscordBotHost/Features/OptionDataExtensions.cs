namespace DiscordBotHost.Features
{
    public static class OptionDataExtensions
    {
        public static bool TryGetValue<T>(this SocketSlashCommand command, string name, out T value, T defaultValue = default!)
        {
            value = defaultValue;
            var option = command.Data.Options?.FirstOrDefault(o => o.Name == name);
            if (option == null || option.Value is not T typedValue)
            {
                return false;
            }

            value = typedValue;
            return true;
        }
    }
}
