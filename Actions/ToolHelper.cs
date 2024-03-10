using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Assistants;

[AttributeUsage(AttributeTargets.Method)]
public class FunctionAttribute : Attribute
{
    public string name { get; set; }
    public string description { get; set; }
    public string type { get; set; } = "function";

    public FunctionAttribute(string name, string description)
    {
        this.name = name;
        this.description = description;
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class ParameterAttribute : Attribute
{
    public string description { get; set; }

    public ParameterAttribute(string description)
    {
        this.description = description;
    }
}

public static class ToolHelpers
{
    public static async Task<string> Run(this ToolCallRequest request)
    {
        var methods = Assembly.GetExecutingAssembly()
            .GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes(typeof(FunctionAttribute), false).Length > 0)
            .ToArray();
        var matchingMethod = methods
            .FirstOrDefault(m => m.GetCustomAttribute<FunctionAttribute>().name == request.function.name);

        if (matchingMethod is not null)
        {
            var arguments = JsonSerializer.Deserialize<Dictionary<string, string>>(request.function.arguments);
            var parameterValues = matchingMethod.GetParameters()
                .Select(p => arguments[p.Name])
                .ToArray();
            var result = matchingMethod.Invoke(null, parameterValues);
            return result.ToString();
        }
        return "done";
    }

    public static Tool[] GetTools()
    {
        var methods = Assembly.GetExecutingAssembly()
            .GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes(typeof(FunctionAttribute), false).Length > 0)
            .ToArray();
        return methods
            .Select(m => new Tool()
            {
                type = "function",
                function = new ToolFunction()
                {
                    name = m.GetCustomAttribute<FunctionAttribute>().name,
                    description = m.GetCustomAttribute<FunctionAttribute>().description,
                    parameters = new ToolParameters()
                    {
                        type = "object",
                        properties = m.GetParameters()
                            .ToDictionary(p => p.Name, p => new ToolParameter()
                            {
                                type = "string",
                                description = p.GetCustomAttribute<ParameterAttribute>().description
                            }),
                        required = m.GetParameters().Select(p => p.Name).ToArray()
                    }
                }
            })
            .ToArray();
    }
}


public static class WindowsCmdCommand
{
    public static void Run(string command, out string output, out string error, string directory = null)
    {
        using Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                Arguments = "/c " + command,
                CreateNoWindow = true,
                WorkingDirectory = directory ?? string.Empty,
            }
        };
        process.Start();
        output = process.StandardOutput.ReadToEnd();
        error = process.StandardError.ReadToEnd();
        process.WaitForExit(TimeSpan.FromSeconds(30));
    }
}