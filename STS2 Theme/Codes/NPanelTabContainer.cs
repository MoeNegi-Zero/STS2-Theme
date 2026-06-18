using Godot;
using System;

public partial class NPanelTabContainer : TabContainer
{
	public event Action OnTabChange;

	public override void _Ready()
	{
		TabChanged += OnTabChanged;
	}
	private void OnTabChanged(long tab)
	{
		OnTabChange?.Invoke();
	}
}
