using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KustoLoco.Core.Console;
using NotNullStrings;

namespace lokqlDx;

public class Copilot
{
    public static class Roles
    {
        public const string User = "user";
        public const string Assistant = "assistant";
        public const string System = "system";
        public const string Kql = "kql";
    }
    public void RenderResponses(IKustoConsole console,params string [] roles)
    {
        foreach(var message in context)
        {
            if (roles.Length > 0 && !roles.Contains(message.role))
            {
                continue;
            }
            var color = message.role switch
            {
                Roles.User => ConsoleColor.Green,
                Roles.Assistant => ConsoleColor.White,
                Roles.System => ConsoleColor.Red,
                Roles.Kql => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };
            console.ForegroundColor = color;
            console.WriteLine(message.content);
            console.WriteLine();
        }
    }

    private HttpClient _client;

    List<ChatMessage> context = [];
    public Copilot(string apiToken)
    {
        Initialised = apiToken.IsNotBlank();
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiToken);
        AddSystemInstructions(@"
You are an assistant to the user. You will be asked questions about tables of data and
will be expected to provide answers in the form of KQL queries.

This KQL engine does not support the top operator. Please avoid it in your responses.

This KQL engine does not support joining on more than a single column. Please avoid doing this in your responses and
instead use the extend operator to generate a new column that can be used for joining.

This KQL implementation does not support arg_min or dynamic property types.

You should provide only a brief explanation of the query and ensure that the KQL query is enclosed within the markdown code block syntax.
Prefer to place each pipeline stage on a new line to improve readability.

When asked to render a chart, use the render operator to specify the type of chart to render.  The render operator should be the final operator in the query.
The only 'with' property that the render operator supports is 'title'.  Please use this to specify a suitable name for the chart.   

When asked to render multi-series charts use the project operator to order columns such that the column that defines the series
name is the final one in the list.  Charts are rendered such that the first column is the x-axis, the second is the y-axis and
the last one is the series name.

In general, prefer to place any time-based column as the x-axis and quantity-based columns as the y-axis but this is not a strict requirement.

Here are some common mistakes I want you to avoid:
- Using the top operator
- Using the arg_min operator
- Using the dynamic property types
- Joining on more than a single column
- Using the render operator in the middle of the query
- using 'm' or 'mon' to specify months in a timespan.  Remember that 'm' is minutes and that 'mon' is not a valid timespan.  Instead you need to convert months to a number of days.

I will now give some some information about the schema of the tables that you will be asked to query.

");
    }


    public async Task<string> Issue(string question)
    {
        AddUserMessage(question);
        var requestData = new
        {
            model = "gpt-4",
            messages = context
                .Where(m=> m.role!= Roles.Kql)
                .Select(x => new { x.role, x.content }).ToArray()
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8,
            "application/json");
        var response = await _client.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
        var responseString = await response.Content.ReadAsStringAsync();
        var responseDto = JsonSerializer.Deserialize<ChatResponse>(responseString);
        await response.Content.ReadFromJsonAsync<ChatResponse>();
        var assistanceResponse = responseDto?.choices?.FirstOrDefault()?.message.content ?? string.Empty;
        AddCopilotResponse(assistanceResponse);
        return assistanceResponse;
    }

    private void AddMessage(string role, string content) => context.Add(new ChatMessage(role, content));

    private void AddUserMessage(string content) => AddMessage(Roles.User, content);

    private void AddCopilotResponse(string content) => AddMessage(Roles.Assistant, content);
    public void AddSystemInstructions(string content) => AddMessage(Roles.System, content);


    public readonly record struct ChatMessage(string role, string content);

    public class ChatResponse
    {
        public ChatChoice[] choices { get; set; } = [];
    }

    public class ChatChoice
    {
        public ChatMessage message { get; set; } = new ChatMessage();
    }

    public bool Initialised { get; private set; }

    public void AddResponse(string response)
    {
       AddMessage(Roles.Kql,response);
    }
}
