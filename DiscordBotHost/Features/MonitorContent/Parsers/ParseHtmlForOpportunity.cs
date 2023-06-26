using OpenAI.Models;

namespace DiscordBotHost.Features.Auditions.Parsers
{
	public class ParseHtmlForOpportunity
	{
		private readonly OpenAIClient ai;

		public ParseHtmlForOpportunity(OpenAIClient ai)
		{
			this.ai = ai;
		}

		public async Task ParseFromHtml(string prompt = "I have 10 minutes to make something cool, I'm going too ")
		{
			Log.Information("AI RESPONSE TO '{prompt}'", prompt);
			await foreach (var token in ai.CompletionsEndpoint.StreamCompletionEnumerableAsync(prompt, maxTokens: 200, temperature: 0.5, presencePenalty: 0.1, frequencyPenalty: 0.1, model: Model.GPT4))
			{
				Log.Information(token);
			}
		}
	}
}
