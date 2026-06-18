using Godot;

[GlobalClass]
public partial class NDraggableWindow : FoldableContainer
{
	private bool _dragging = false;
	private Vector2 _dragOffset;

	public override void _Ready()
	{
		Connect("gui_input", new Callable(this, nameof(OnDrag)));
		FoldingChanged += OnFoldingChanged;

		GetNode<NPanelTabContainer>("%PanelTabContainer").OnTabChange += OnTabButtonDown;
		GetNode<NNodesFolder>("%MainMenuNodesFolder").OnFolding += OnTabButtonDown;
		GetNode<NNodesFolder>("%MainMenuBgNodesFolder").OnFolding += OnTabButtonDown;
		GetNode<NPropertyPanel>("%PropertyPanel").OnFolding += OnTabButtonDown;
	}

	private void OnDrag(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb)
		{
			if (mb.ButtonIndex == MouseButton.Left)
			{
				if (mb.Pressed)
				{
					_dragging = true;
					_dragOffset = GetGlobalMousePosition() - GlobalPosition;
				}
				else
				{
					if (_dragging)
						EndDrag();

					_dragging = false;
				}
			}
		}
		else if (@event is InputEventMouseMotion mm)
		{
			if (_dragging)
			{
				GlobalPosition = GetGlobalMousePosition() - _dragOffset;
			}
		}
	}

	private void OnFoldingChanged(bool is_folded)
	{
		CustomMinimumSize = Vector2.Zero;
		Size = new Vector2(484, 0);
	}

	private void EndDrag()
	{
		_dragging = false;

		CustomMinimumSize = Vector2.Zero;

		Size = new Vector2(484, 0);

		QueueSort();
	}

	private void OnTabButtonDown()
	{
		CustomMinimumSize = Vector2.Zero;

		Size = new Vector2(484, 0);

		QueueSort();
	}
}
