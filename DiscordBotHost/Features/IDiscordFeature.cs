using Discord.Net;

using MediatR;

using IRequest = MediatR.IRequest;

namespace DiscordBotHost.Features
{
	public interface IDiscordFeature
	{
		string FeaturePrefix { get; }

		Task<IRequest> Commands<TResponse>(ulong userId = 0);

		Task<IRequest<TResponse>> Queries<TResponse>(ulong userId = 0);

		Task<INotification> Events();
		
		Task AddCommands();
		Task RemoveCommands();

		Task AddQueries();
		Task Subscribe();
		Task Unsubscribe();

		Task AddSlashCommands(DiscordSocketClient client);

		Task RemoveSlashCommands(DiscordSocketClient client);

		Task AddEventHandlers(DiscordSocketClient client);
		
		Task RemoveEventHandlers(DiscordSocketClient client);

		Task AddServices(DiscordSocketClient client);

		Task AddHttpEndpoints(object endpointRouteBuilder);

	}
}
