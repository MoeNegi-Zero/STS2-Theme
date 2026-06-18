using Godot;
using MegaCrit.Sts2.Core.Models.Cards;
using System;
using static Godot.Control;

public partial class NEditableFrame : Panel
{
    public string ItemName { get; set; } = "";
    public Node TargetNode { get; private set; }

    private bool _dragging = false;
    private Vector2 _dragOffset;
    public TextureRect pivot;

    public event Action<string> FrameSelected;

    [Signal] public delegate void OnDraggingEventHandler(NEditableFrame frame);
    [Signal] public delegate void AfterDraggingEventHandler(NEditableFrame frame);

    private static readonly StyleBox SelectedStyle = ResourceLoader.Load<StyleBox>("res://STS2 Theme/Themes/NEditableFrame_selected.tres");
    private static readonly CompressedTexture2D pivotIcon = ResourceLoader.Load<CompressedTexture2D>("res://STS2 Theme/Images/Editor/UI/EditorPivot.svg");

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        Connect("gui_input", new Callable(this, nameof(OnDrag)));
        
        if (TargetNode is Control)
        {
            var size = (Vector2)TargetNode.Get(Control.PropertyName.Size);
            Vector2 pivotoffset = (Vector2)TargetNode.Get(Control.PropertyName.PivotOffset);
            var pivotSize = Math.Min(size.X, size.Y) / 8;

            pivot = new TextureRect();
            pivot.CustomMinimumSize = new Vector2(2, 2);
            pivot.Size = new Vector2(pivotSize, pivotSize).Max(32).Min(16);
            pivot.Position = pivotoffset - pivot.Size / 2;
            pivot.PivotOffset = new Vector2(8, 8);
            pivot.Texture = pivotIcon;
            pivot.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;

            AddChild(pivot);
        }
    }

    public void Initialize(string name, Node targetNode)
    {
        TargetNode = targetNode;
    }

    public void Select()
    {
        AddThemeStyleboxOverride("panel", SelectedStyle);
        MoveToFront();
    }

    public void Deselect()
    {
        RemoveThemeStyleboxOverride("panel");
    }

    private void OnDrag(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            _dragging = mb.Pressed;
            if (_dragging)
                _dragOffset = GetGlobalMousePosition() - GlobalPosition;
            else
                EmitSignal("AfterDragging", this);
        }
        else if (@event is InputEventMouseMotion mm && _dragging)
        {
            GlobalPosition = GetGlobalMousePosition() - _dragOffset;
            EmitSignal("OnDragging", this);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
             FrameSelected?.Invoke(ItemName);
        }
    }
}
