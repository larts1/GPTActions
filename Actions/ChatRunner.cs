using System.Net.Http.Json;
using System.Text.Json;

namespace Assistants;

public class ChatRunner
{
    private string thread_id { get; set; }
    private string assistant_id { get; set; } = "asst_flhZNoyy5esTjZwoHExUYelq";
    private HttpClient client { get; set; }

    public ChatRunner()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
        client.BaseAddress = new Uri("https://api.openai.com/v1/");
    }

    public async Task SyncAssistant()
    {
        var assistantSync = await client.PostAsJsonAsync($"assistants/{assistant_id}", new
        {
            instructions = "Autat kehittäjää komentotulkin välityksellä",
            tools = ToolHelpers.GetTools(),
            model = "gpt-3.5-turbo"
        });
        if (!assistantSync.IsSuccessStatusCode)
        {
            var tools = ToolHelpers.GetTools();
            var toolsJson = JsonSerializer.Serialize(tools);
            var content = await assistantSync.Content.ReadAsStringAsync();
            Console.WriteLine("Failed to sync assistant");
        }
    }

    public async Task<string> Ask(string question)
    {
        Run run = null;
        if (thread_id is null)
        {
            var createMessage = await client.PostAsJsonAsync($"threads/{thread_id}/runs", new
            {
                assistant_id = assistant_id,
                instructions = "Olet avulias komentotulkkini",
                thread = new
                {
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = question
                        }
                    }
                }
            });
            if (!createMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to create message");
                return "Failed to create message";
            }
            run = await createMessage.Content.ReadFromJsonAsync<Run>();
            thread_id = run.thread_id;
        }
        else
        {
            var createMessage = await client.PostAsJsonAsync($"threads/{thread_id}/messages", new
            {
                role = "user",
                content = question
            });
            if (!createMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to create message");
                return "Failed to create message";
            }
            var runRequest = await client.PostAsJsonAsync($"threads/{thread_id}/runs", new RunStart
            {
                assistant_id = assistant_id,
                additional_instructions = null
            });
            if (!runRequest.IsSuccessStatusCode)
            {
                return "Failed to create run";
            }
            run = await runRequest.Content.ReadFromJsonAsync<Run>();
        }

        while (true)
        {
            var getRun = await client.GetAsync($"threads/{thread_id}/runs/{run.id}");
            run = await getRun.Content.ReadFromJsonAsync<Run>();
            if (run is null)
            {
                Console.WriteLine("Failed to create run");
                return "Failed to create run";
            }
            switch (run.status)
            {
                case "queued":
                case "in_progress":
                case "running":
                    Thread.Sleep(500);
                    break;
                case "completed":
                    var messages = await client.GetAsync($"threads/{run.thread_id}/messages");
                    var messagesObj = await messages.Content.ReadFromJsonAsync<ThreadMessages>();
                    var lastMessage = messagesObj.data.First().content.Last();
                    return lastMessage.text.value;
                case "requires_action":
                    var outputDataAsync = run.required_action.submit_tool_outputs.tool_calls.Select(async x =>
                        new
                        {
                            tool_call_id = x.id,
                            output = await x.Run()
                        }
                    );
                    var outputData = await Task.WhenAll(outputDataAsync);
                    var submitToolOutput = await client.PostAsJsonAsync(
                        $"threads/{thread_id}/runs/{run.id}/submit_tool_outputs", new
                        {
                            tool_outputs = outputData
                        });
                    if (!submitToolOutput.IsSuccessStatusCode)
                    {
                        return "Failed to submit tool output";
                    }

                    break;
                case "failed":
                    var getRunx = await client.GetAsync($"threads/{thread_id}/runs/{run.id}/steps");
                    var runx = await getRunx.Content.ReadAsStringAsync();
                    return run.last_error.code + " " + run.last_error.message;
                default:
                    return "Unknown state " + run.status;
            }
        }
    }
}
