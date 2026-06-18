using MegaCrit.Sts2.Core.Logging;
using System.Collections.Generic;
using System.IO;

public static class ResourceHandler
{
    public static Dictionary<string, string> _map = new();

    public static void LoadFromCfg(string path)
    {
        if (!File.Exists(path)) return;
        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("["))
                continue;

            var kv = line.Split('=', 2);
            if (kv.Length == 2)
                _map[kv[0].Trim()] = kv[1].Trim();
        }

        LogAllItem();
    }

    public static void LogAllItem()
    {
        foreach (var key in _map.Keys)
        {
            Log.Info($"Key = {key}  Value = {_map[key]}");
        }
    }

    public static bool TryGet(string key, out string resourcePath)
        => _map.TryGetValue(key, out resourcePath);
}