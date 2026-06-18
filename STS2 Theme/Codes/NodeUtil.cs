using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.Window;

public static class NodeUtil
{
    public static void SyncFromNode(Node owner, Node target)
    {
        if (owner is Control && target is Control)
        {
            Vector2 size = (Vector2)owner.Get(Control.PropertyName.Size);
            target.Set(Control.PropertyName.Size, size);

            Vector2 pivot = (Vector2)owner.Get(Control.PropertyName.PivotOffset);
            target.Set(Control.PropertyName.PivotOffset, pivot);
        }

        Vector2 position = (Vector2)owner.Get(Control.PropertyName.Position);
        target.Set(Control.PropertyName.Position, position);

        float rotation = (float)owner.Get(Control.PropertyName.Rotation);
        target.Set(Control.PropertyName.Rotation, rotation);

        Vector2 scale = (Vector2)owner.Get(Control.PropertyName.Scale);
        target.Set(Control.PropertyName.Scale, scale);

        StringName[] namelist = [
            Control.PropertyName.GlobalPosition
        ];

        foreach (StringName name in namelist)
        {
            var value = owner.Get(name);
            target.Set(name, value);
        }
    }

    public static void SyncToNode(Node owner, Node target)
    {
        SyncFromNode(target, owner);
    }

    public static T? FindFirstNodeOfType<T>(Node root) where T : Node
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is T typed)
                return typed;

            T? found = FindFirstNodeOfType<T>(child);
            if (found != null)
                return found;
        }

        return null;
    }

    /*
    public static string BuildID(Node node)
    {
        var id = node.GetPathTo(node.Owner) + "/" + node.Name;

        return id;
    }*/

    public static string BuildID(Node node)
    {
        if (node.Owner == null)
            return node.Name;

        if (node == node.Owner)
            return ".";

        var stack = new Stack<string>();
        var current = node;

        while (current != null && current != node.Owner)
        {
            stack.Push(current.Name);
            current = current.GetParent();
        }

        if (current != node.Owner)
        {
            // 不在当前场景树内
            return null;
        }

        return node.Owner.Name + "/" + string.Join("/", stack);
    }

    public static NodeKey BuildKey(Node node)
    {
        string? parent = node.GetParent() != null
            ? node.GetParent().Name
            : null;

        return new NodeKey(
            parent: parent,
            name: node.Name,
            type: node.GetType().Name
        );
    }
}