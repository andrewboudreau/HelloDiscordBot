namespace DiscordBotHost.EntityFramework
{
	public class User
	{
		public static User Empty { get; } = new User(0, "", 0, "", 0);

		private User(ulong id, string name, ulong discordUserId, string firebaseId, ulong linksChannelId)
		{
			Id = id;
			Name = name;
			DiscordUserId = discordUserId;
			FirebaseId = firebaseId;
			LinksChannelId = linksChannelId;
		}

		public ulong Id { get; private set; }
		public string Name { get; private set; }
		public ulong DiscordUserId { get; private set; }
		public string FirebaseId { get; private set; }
		public ulong LinksChannelId { get; private set; }

		public static User Create(string name, ulong discordId, ulong linksChannel = Globals.DefaultChannelId)
			=> new(0, name, discordId, "", linksChannel);

		public void SetLinksChannelId(ulong channelId)
		{
			LinksChannelId = channelId;
		}
	}
}
