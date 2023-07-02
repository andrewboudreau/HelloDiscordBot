using OpenAI.Chat;

using System.Text.Json;
using System.Text.Json.Nodes;

namespace DiscordBotHost.Features.Auditions.Parsers
{
	public class JobsArgs
	{
		public List<Job> Jobs { get; set; }
	}

	public class Job
	{
		public string Company { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public string Description { get; set; }
		public string Date { get; set; }
		public string AuditionEndDate { get; set; }
		public string? Url { get; set; }
	}

	public class ParseTextContentForAuditionCall
	{
		private readonly OpenAIClient ai;

		public ParseTextContentForAuditionCall(OpenAIClient ai)
		{
			this.ai = ai;
		}

		public async Task<IEnumerable<Job>> ParseForAuditionCalls(string prompt)
		{
			Log.Information("AI RESPONSE TO '{prompt}'", prompt);

			var messages = new List<Message>
			{
				new Message(Role.System, "You parse text and html and auditions and jobs"),
				new Message(Role.User, $"Create any jobs found in {prompt}"),
			};

			var functions = new List<Function>
			{
				new Function(
					name: "create_job",
					description: "Create an job listing",
					parameters: new JsonObject
					{
						["type"] = "object",
						["properties"] = new JsonObject
						{
							["jobs"] = new JsonObject
							{
								["type"] = "array",
								["items"] = new JsonObject
								{
									["type"] = "object",
									["properties"] = new JsonObject
									{
										["company"] = new JsonObject
										{
											["type"] = "string",
											["description"] = "Name of the theater or production company"
										},
										["url"] = new JsonObject
										{
											["type"] = "string",
											["description"] = "The url to view more details"
										},
										["name"] = new JsonObject
										{
											["type"] = "string",
											["description"] = "Name of the performance, musical, play, or opera"
										},
										["type"] = new JsonObject
										{
											["type"] = "string",
											["enum"] = new JsonArray {"film", "play", "musical", "choir", "singer", "other"}
										},
										["description"] = new JsonObject
										{
											["type"] = "string",
											["description"] = "Description of the opportunity"
										},
										["date"] = new JsonObject
										{
											["type"] = "string",
											["description"] = "Audition start date and time in ISO 8601"
										}
									}
								}
							}
						},
						["required"] = new JsonArray { "jobs" }
					})
			};

			ChatResponse? result = null;
			var chatRequest = new ChatRequest(messages, functions: functions, functionCall: "auto", model: "gpt-4");
			try
			{
				result = await ai.ChatEndpoint.StreamCompletionAsync(chatRequest,
				fragment =>
				{
					Log.Information("ChatResponseCost: still working. {usage}", fragment.GetHashCode());
				});
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error calling open ai");
				//return $"Error calling open ai, {ex.Message}";
				return Enumerable.Empty<Job>();
			}

			if (result is null)
			{
				Log.Error("The result from open ai was null");
				return Enumerable.Empty<Job>();
				//return "The result from open ai was null";
			}

			Log.Information("========Done=============");
			Log.Information(result.FirstChoice.Message.Function.Name);
			Log.Information(result.FirstChoice.Message.Function.Arguments.ToJsonString());

			var jobs = JsonSerializer.Deserialize<JobsArgs>(
				result.FirstChoice.Message.Function.Arguments.ToJsonString(),
				new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

			return jobs?.Jobs ?? Enumerable.Empty<Job>();
		}
	}
}
