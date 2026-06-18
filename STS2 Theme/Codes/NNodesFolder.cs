using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class NNodesFolder : FoldableContainer
{
	public event Action<Node, VBoxContainer> OnRightClicked;
	public event Action OnFolding;

	public override void _Ready()
	{
		FoldingChanged += OnFoldingChanged;
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb &&
			mb.ButtonIndex == MouseButton.Right &&
			mb.Pressed)
		{
			switch (this.Name)
			{
				case "MainMenuNodesFolder":
					OnRightClicked?.Invoke(NMainMenuPatch.nMainMenu, this.GetNode<VBoxContainer>("MainMenuNodes"));
					break;

				case "MainMenuBgNodesFolder":
					OnRightClicked?.Invoke(NMainMenuPatch.nMainMenuBg.GetNode("BgContainer"), this.GetNode<VBoxContainer>("MainMenuBgNodes"));
					break;
			}
		}
	}
	private void OnFoldingChanged(bool is_folded)
	{
		CustomMinimumSize = new Vector2(0, 0);
		if (is_folded) OnFolding?.Invoke();
	}
}
