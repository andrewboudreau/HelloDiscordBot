namespace DiscordBotHost.EntityFramework
{
	public class DiscordUser
	{
		public static DiscordUser Empty { get; } 
			= new DiscordUser(0, "", 0, "", 0);

		public DiscordUser(int id, string name, ulong discordId, string firebaseId, ulong linksChannelId)
		{
			Id = id;
			Name = name;
			DiscordId = discordId;
			FirebaseId = firebaseId;
			LinksChannelId = linksChannelId;
		}

		public int Id { get; private set; }
		public string Name { get; private set; }
		public ulong DiscordId { get; private set; }
		public string FirebaseId { get; private set; }
		public ulong LinksChannelId { get; private set; }

		public static DiscordUser Create(string name, ulong discordId, ulong linksChannel = Globals.DefaultChannelId)
			=> new(0, name, discordId, "", linksChannel);

		internal void SetLinksChannelId(ulong channelId)
		{
			LinksChannelId = channelId;
		}
	}
}
