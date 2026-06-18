using System.Text.RegularExpressions;

public static class TscnKeyExtractor
{
    private static readonly Regex NodeRegex =
        new(@"^\[node name=""(?<name>.+?)""( type=""(?<type>.+?)"")?( parent=""(?<parent>.*?)"")?( instance=.+)?");


    public static bool TryExtractKey(string line, out NodeKey key)
    {
        key = default;

        var match = NodeRegex.Match(line);
        if (!match.Success)
            return false;

        string name = match.Groups["name"].Value;

        string? parent =
            match.Groups["parent"].Success
                ? match.Groups["parent"].Value
                : null; // ✔ root

        bool isInstance = line.Contains("instance=");

        string type =
            isInstance
                ? "__instance__"
                : match.Groups["type"].Value;

        key = new NodeKey(parent, name, type);
        return true;
    }
}