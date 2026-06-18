using Godot;

public partial class NUnSelectableNode : Control
{
    public string _title;
    public string ItemName { get; set; } = "";

    public override void _Ready()
    {
        GetNode<Label>("%Title").Text = _title;
    }
}
