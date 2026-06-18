using System;
using System.Collections.Generic;
using System.IO;

public static class ConfigFile
{
    private static readonly string cfgPath = Path.Combine(ThemePaths.ModDirectory, "STS2 Theme.cfg");

    // 内部缓存
    private static Dictionary<string, string>? _cache;

    /// <summary>
    /// 加载配置文件到缓存，如果已经加载过则直接返回缓存
    /// </summary>
    private static Dictionary<string, string> LoadCache()
    {
        if (_cache != null)
            return _cache;

        _cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(cfgPath))
            return _cache;

        foreach (var line in File.ReadAllLines(cfgPath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("["))
                continue;

            var kv = line.Split('=', 2);
            if (kv.Length == 2)
                _cache[kv[0].Trim()] = kv[1].Trim();
        }

        return _cache;
    }

    /// <summary>
    /// 根据键获取值，如果不存在返回 null
    /// </summary>
    public static string? GetValue(string key)
    {
        var dict = LoadCache();
        dict.TryGetValue(key, out var value);
        return value;
    }

    /// <summary>
    /// 设置键值，如果存在则更新，否则追加
    /// </summary>
    public static void SetValue(string key, string value)
    {
        var dict = LoadCache();

        // 更新缓存
        dict[key] = value;

        // 确保目录存在
        Directory.CreateDirectory(Path.GetDirectoryName(cfgPath)!);

        // 写回文件
        using var writer = new StreamWriter(cfgPath, false);
        writer.WriteLine("[STS2 ThemeConfig]");
        foreach (var kvp in dict)
        {
            writer.WriteLine($"{kvp.Key}={kvp.Value}");
        }
    }

    /// <summary>
    /// 删除某个键
    /// </summary>
    public static void RemoveValue(string key)
    {
        var dict = LoadCache();
        if (dict.Remove(key))
        {
            // 写回文件
            using var writer = new StreamWriter(cfgPath, false);
            writer.WriteLine("[STS2 ThemeConfig]");
            foreach (var kvp in dict)
            {
                writer.WriteLine($"{kvp.Key}={kvp.Value}");
            }
        }
    }

    /// <summary>
    /// 清空缓存（例如文件被外部修改后）
    /// </summary>
    public static void Reload()
    {
        _cache = null;
    }
}