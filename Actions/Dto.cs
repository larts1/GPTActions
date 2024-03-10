using System.Text.Json.Serialization;

namespace Assistants;

public class RunStart
{
    public string assistant_id { get; set; }
    public string additional_instructions { get; set; }
}

public class Run
{
    public string id { get; set; }
    public string Object { get; set; }
    public int? created_at { get; set; }
    public string assistant_id { get; set; }
    public string thread_id { get; set; }
    public string status { get; set; }
    public int? started_at { get; set; }
    public int? expires_at { get; set; }
    public int? cancelled_at { get; set; }
    public int? failed_at { get; set; }
    public int? completed_at { get; set; }
    public ToolCall required_action { get; set; }
    public RunError last_error { get; set; }
    public string model { get; set; }
    public string instructions { get; set; }
    //public Tool[] tools { get; set; }
    public string[] file_ids { get; set; }
    public object metadata { get; set; }
    public object usage { get; set; }

}

public class RunError
{
    public string message { get; set; }
    public string code { get; set; }
}

public class ToolCall
{
    public string type { get; set; }
    public ToolCallRequestObj submit_tool_outputs { get; set; }
}

public class ToolCallRequestObj
{
    public ToolCallRequest[] tool_calls { get; set; }
}

public class ToolCallRequest
{
    public string id { get; set; }
    public string type { get; set; }
    public ToolFunctionCall function { get; set; }
}

public class ToolFunctionCall
{
    public string name { get; set; }
    public string arguments { get; set; }
}

public class Tool
{
    public string type { get; set; } = "function";
    public ToolFunction function { get; set; }
}

public class ToolFunction
{
    public string name { get; set; }
    public string description { get; set; }
    public ToolParameters parameters { get; set; }
}

public class ToolParameters
{
    public string type { get; set; }
    public Dictionary<string, ToolParameter> properties { get; set; } = new();
    public string[] required { get; set; } = new string[] { };
}

public class ToolParameter
{
    public string type { get; set; }
    public string description { get; set; }
    // [JsonPropertyName("enum")]
    // public string[] Enum { get; set; }
}

public class ThreadMessages
{
    public ThreadMessage[] data { get; set; }
}

public class ThreadMessage
{
    public string id { get; set; }
    // public string Object { get; set; }
    public int? created_at { get; set; }
    public string thread_id { get; set; }
    public string role { get; set; }
    public Content[] content { get; set; }
    public string[] file_ids { get; set; }
    public string assistant_id { get; set; }
    public string run_id { get; set; }
    public object metadata { get; set; }
}

public class ThreadCreation
{
    public string id { get; set; }
}

public class Content
{
    public string type { get; set; }
    public Text text { get; set; }
}

public class Text
{
    public string value { get; set; }
    public object[] annotations { get; set; }
}