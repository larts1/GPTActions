namespace Assistants;

public class WebsiteOpener
{
    [Function(
        name: "open_website",
        description: "Opens a website in the default browser")
    ]
    public static string OpenWebsite(
        [Parameter("The URL of the website to open")] string url)
    {
        WindowsCmdCommand.Run("start " + url, out var output, out _);
        return output;
    }
}