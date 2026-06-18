using MegaCrit.Sts2.Core.Logging;
using System.IO;
using System.Text.RegularExpressions;

public static class TscnExtResourcePatcher
{
    // 匹配 node 行中的 ExtResource("id")
    private static readonly Regex NodeResourceRegex =
        new(@"instance\s*=\s*ExtResource\(""(?<id>[^""]+)""\)", RegexOptions.Compiled);

    // 匹配 ext_resource 行
    private static readonly Regex ExtResourceRegex =
       new(@"^\[ext_resource\s+type=""(?<type>[^""]+)""\s+path=""(?<path>[^""]+)""\s+id=""(?<id>[^""]+)""\]$", RegexOptions.Compiled);

    public static void ReplaceNodeResourcePath(
        string tscnPath,
        string nodeName,
        string newPath)
    {
        var lines = File.ReadAllLines(tscnPath);

        // 1. 找 node → resource id
        string targetResourceId = null;

        foreach (var line in lines)
        {
            if (!line.Contains($"name=\"{nodeName}\""))
                continue;

            var match = NodeResourceRegex.Match(line);
            if (match.Success)
            {
                targetResourceId = match.Groups["id"].Value;
                Log.Info(">>>[STS2 Theme id=]"+targetResourceId);
                break;
            }
        }

        if (string.IsNullOrEmpty(targetResourceId))
        {
            // 没有 instance 说明不是 resource node（比如纯 Control）
            return;
        }

        // 2. 修改 ext_resource
        for (int i = 0; i < lines.Length; i++)
        {
            var match = ExtResourceRegex.Match(lines[i]);
            if (!match.Success)
                continue;

            var id = match.Groups["id"].Value;

            if (id == targetResourceId)
            {
                lines[i] = Regex.Replace(
                    lines[i],
                    @"path\s*=\s*""[^""]+""",
                    $"path=\"{newPath}\""
                );

                break;
            }
        }

        File.WriteAllLines(tscnPath, lines);
    }
}