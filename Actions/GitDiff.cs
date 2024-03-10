namespace Assistants;

public class GitDif
{
    [Function(
        name: "git_diff",
        description: "Shows the difference between current branch and origin main"
    )]
    public static string GitDiff()
    {
        WindowsCmdCommand.Run("git diff origin/main", out var output, out _);
        return output;
    }
}