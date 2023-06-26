﻿using DiscordBotHost.EntityFramework;

using Microsoft.EntityFrameworkCore;

namespace DiscordBotHost.Features
{
	public static class SocketUserExtensions
	{
		public static async Task<User> GetOrCreateAsync(this DiscordBotDbContext dbContext, SocketUser user)
		{
			var entity = await dbContext.Users
				.Where(x => x.DiscordUserId == user.Id)
				.FirstOrDefaultAsync();

			return entity ??= 
				dbContext.Add(User.Create(user.Username, user.Id)).Entity;
		}

		public static async Task<User> SetLinksChannel(this DiscordBotDbContext dbContext, SocketUser socketUser, ulong channelId)
		{
			var user = await dbContext.GetOrCreateAsync(socketUser);
			user.SetLinksChannelId(channelId);
			await dbContext.SaveChangesAsync();

			return user;
		}
	}
}
