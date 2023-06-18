namespace DiscordBotHost.Commands.LinksChannel
{
	public static class SharedLinksCommandDefinitions
    {
        public static IEnumerable<SlashCommandProperties> SetLinksChannelCommands()
        {
            yield return new SlashCommandBuilder()
                .WithName("SetLinksChannel")
                .WithDescription("Sets the links channel.")
                .AddOption(
                    name: "channel",
                    type: ApplicationCommandOptionType.Channel,
                    description: "The channel to set as the links channel",
                    isRequired: true)
                .Build();

            yield return new SlashCommandBuilder()
                .WithName("ListLinksChannel")
                .WithDescription("Lists the current links channel.")
                .Build();
        }
    }
}
