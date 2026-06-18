using Godot;
using System;
using System.Collections.Generic;

public partial class NSelectableNode : Control
{
	public Node _node;
	public NEditableFrame? _frame;
	public string _title;
	public bool couldRemove;
	public bool couldSelectFile = false;
	public string ItemName { get; set; } = "";

	private PanelContainer _highLight;
	private TextureButton _up;
	private TextureButton _down;
	private TextureButton _remove;
	private TextureButton _visible;
	private TextureButton _file;

	[Signal] public delegate void NodeSelectedEventHandler(string itemName);

	public event Action<string,NSelectableNode> BeforeQueueFree;
	public event Action<Node> OnSelectFileButtonPressed;
	public event Action<NSelectableNode> OnVisibleButtonPressed;

	public override void _Ready()
	{
		_highLight = GetNode<PanelContainer>("%Highlight");
		_up = GetNode<TextureButton>("%Up");
		_down = GetNode<TextureButton>("%Down");
		_remove = GetNode<TextureButton>("%Remove");
		_file = GetNode<TextureButton>("%File");
		GetNode<Label>("%Title").Text = _title;
		_visible = GetNode<TextureButton>("%Visible");
		_visible.Pressed += OnVisibleButtonDown;

		if (_node != NMainMenuPatch.nMainMenuBg.GetNode("%Logo"))
		{
			_up.Connect(TextureButton.SignalName.Pressed, new Callable(this, nameof(OnUpButtonPressed)));
			_down.Connect(TextureButton.SignalName.Pressed, new Callable(this, nameof(OnDownButtonPressed)));
		}
		else
		{
			_up.Connect(TextureButton.SignalName.Pressed, new Callable(this, nameof(OnLogoUpButtonPressed)));
			_down.Connect(TextureButton.SignalName.Pressed, new Callable(this, nameof(OnLogoDownButtonPressed)));
		}

		if (couldRemove)
		{
			//_remove.Visible = true;
			_remove.Connect(TextureButton.SignalName.Pressed, new Callable(this, nameof(OnRemoveButtonPressed)));
		}
		else
			_remove.Visible = false;

		if (couldSelectFile)
		{
			_file.Visible = true;
			_file.Connect(TextureButton.SignalName.Pressed, new Callable(this, nameof(SelectFileButtonPressed)));
		}
		else
		{
			_file.Visible = false;
		}
	}

	private void OnVisibleButtonDown()
	{
		_node.Set("visible", !(bool)_node.Get("visible"));
		OnVisibleButtonPressed?.Invoke(this);
	}

	private void OnUpButtonPressed()
	{
		_node.GetParent().MoveChild(_node, Math.Max(0, _node.GetIndex() - 1));
		this.GetParent().MoveChild(this, Math.Max(0, this.GetIndex() - 1));
	}

	private void OnLogoUpButtonPressed()
	{
		var parent = _node.GetParent();
		parent.GetParent().MoveChild(parent, Math.Max(0, parent.GetIndex() - 1));
		this.GetParent().MoveChild(this, Math.Max(0, this.GetIndex() - 1));
	}

	private void OnDownButtonPressed()
	{
		_node.GetParent().MoveChild(_node, Math.Min(_node.GetParent().GetChildCount()-1, _node.GetIndex() + 1));
		this.GetParent().MoveChild(this, Math.Min(this.GetParent().GetChildCount()-1, this.GetIndex() + 1));
	}

	private void OnLogoDownButtonPressed()
	{
		var parent = _node.GetParent();
		parent.GetParent().MoveChild(parent, Math.Min(parent.GetParent().GetChildCount() - 1, parent.GetIndex() + 1));
		this.GetParent().MoveChild(this, Math.Min(this.GetParent().GetChildCount() - 1, this.GetIndex() + 1));
	}

	private void OnRemoveButtonPressed()
	{
		BeforeQueueFree?.Invoke(_node.Name, this);
		_node.QueueFree();
		this.QueueFree();
	}

	private void SelectFileButtonPressed()
	{
		OnSelectFileButtonPressed?.Invoke(_node);
	}

	public void Select() => _highLight.Visible = true;
	public void Deselect() => _highLight.Visible = false;

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
		{
			EmitSignal("NodeSelected", ItemName);
		}
	}
}
