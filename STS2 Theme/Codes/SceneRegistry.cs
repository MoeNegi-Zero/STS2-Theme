using Godot;
using System.Collections.Generic;
using System.IO;
using FileAccess = Godot.FileAccess;

public class SceneRegistry
{
    private readonly HashSet<NodeKey> _keys = new();

    public IReadOnlyCollection<NodeKey> Keys => _keys;

    public void Add(NodeKey key)
        => _keys.Add(key);

    public bool Remove(NodeKey key)
        => _keys.Remove(key);

    public bool Contains(NodeKey key)
        => _keys.Contains(key);

    private static IEnumerable<string> ReadTscnLines(string path)
    {
        return File.ReadLines(path);
    }

    private static SceneRegistry BuildFromLines(IEnumerable<string> lines)
    {
        var registry = new SceneRegistry();

        foreach (var line in lines)
        {
            if (TscnKeyExtractor.TryExtractKey(line, out var key))
            {
                registry.Add(key);
            }
        }

        return registry;
    }

    public SceneRegistry BuildFromTscn(string tscnPath)
    {
        if (!FileAccess.FileExists(tscnPath))
            return new SceneRegistry();

        using var file = FileAccess.Open(tscnPath, FileAccess.ModeFlags.Read);

        if (file == null)
            return new SceneRegistry();

        var text = file.GetAsText();
        var lines = text.Split('\n');

        return BuildFromLines(lines);
    }
}