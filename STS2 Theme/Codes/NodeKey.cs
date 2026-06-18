using System;

public readonly struct NodeKey : IEquatable<NodeKey>
{
    public readonly string? Parent; // root = null
    public readonly string Name;
    public readonly string Type;

    public NodeKey(string? parent, string name, string type)
    {
        Parent = parent;
        Name = name;
        Type = type;
    }

    public bool Equals(NodeKey other)
        => Parent == other.Parent
        && Name == other.Name
        && Type == other.Type;

    public override int GetHashCode()
        => HashCode.Combine(Parent, Name, Type);
}