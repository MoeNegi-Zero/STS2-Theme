using Godot;
using MegaCrit.Sts2.Core.Models.Cards;
using System;
using System.Drawing;

public partial class NPropertyPanel : FoldableContainer
{
	private EditableItem? _item;
	public bool shouldSync = true;

	private SpinBox _sizeX, _sizeY;
	private SpinBox _positionX, _positionY;
	private SpinBox _rotation;
	private SpinBox _scaleX, _scaleY;
	private SpinBox _pivotX, _pivotY;

	private HBoxContainer _sizeContainer;
	private HBoxContainer _pivotContainer;

	public event Action OnFolding;

	public override void _Ready()
	{
		_sizeContainer = GetNode<HBoxContainer>("%SizeContainer");
		_pivotContainer = GetNode<HBoxContainer>("%PivotContainer");

		_sizeX = GetNode<SpinBox>("%SizeX");
		_sizeY = GetNode<SpinBox>("%SizeY");
		_positionX = GetNode<SpinBox>("%PositionX");
		_positionY = GetNode<SpinBox>("%PositionY");
		_rotation = GetNode<SpinBox>("%Rotation");
		_scaleX = GetNode<SpinBox>("%ScaleX");
		_scaleY = GetNode<SpinBox>("%ScaleY");
		_pivotX = GetNode<SpinBox>("%PivotX");
		_pivotY = GetNode<SpinBox>("%PivotY");

		_sizeX.Connect(SpinBox.SignalName.ValueChanged, new Callable(this, nameof(OnSizeChanged)));
		_sizeY.Connect(SpinBox.SignalName.ValueChanged, new Callable(this, nameof(OnSizeChanged)));
		_positionX.Connect(SpinBox.SignalName.ValueChanged, new Callable(this, nameof(OnPositionChanged)));
		_positionY.Connect(SpinBox.SignalName.ValueChanged, new Callable(this, nameof(OnPositionChanged)));
		_rotation.Connect(SpinBox.SignalName.ValueChanged, new Callable(this, nameof(OnRotationChanged)));
		_scaleX.Connect(SpinBox.SignalName.ValueChanged, new Callable(this, nameof(OnScaleChanged)));
		_scaleY.Connect(SpinBox.SignalName.ValueChanged, new Callable(this, nameof(OnScaleChanged)));
		_pivotX.Connect(SpinBox.SignalName.ValueChanged, new Callable(this, nameof(OnPivotChanged)));
		_pivotY.Connect(SpinBox.SignalName.ValueChanged, new Callable(this, nameof(OnPivotChanged)));

		FoldingChanged += OnFoldingChanged;
	}

	public void ShowProperties(EditableItem item)
	{
		shouldSync = false;
		_item = item;

		Node node = item.TargetNode;

		if (node is Control)
		{
			Vector2 size = (Vector2)node.Get(Control.PropertyName.Size);
			_sizeContainer.Visible = true;
			_sizeX.Value = size.X;
			_sizeY.Value = size.Y;

			Vector2 pivot = (Vector2)node.Get(Control.PropertyName.PivotOffset);
			_pivotContainer.Visible = true;
			_pivotX.Value = pivot.X;
			_pivotY.Value = pivot.Y;

			if (item.Frame != null)
			{
				var pivotSize = Math.Min(size.X, size.Y) / 8;
				_item.Frame.pivot.Size = new Vector2(pivotSize, pivotSize).Max(32).Min(16);
				_item.Frame.pivot.Position = pivot - _item.Frame.pivot.Size/2;
			}
		}
		else
		{
			_sizeContainer.Visible = false;
			_sizeX.Value = default(double);
			_sizeY.Value = default(double);

			_pivotContainer.Visible = false;
			_pivotX.Value = default(double);
			_pivotY.Value = default(double);
		}

		Vector2 position = (Vector2)node.Get(Control.PropertyName.Position);
		_positionX.Value = position.X;
		_positionY.Value = position.Y;

		float rotation = (float)node.Get(Control.PropertyName.RotationDegrees);
		_rotation.Value = rotation;

		Vector2 scale = (Vector2)node.Get(Control.PropertyName.Scale);
		_scaleX.Value = scale.X;
		_scaleY.Value = scale.Y;

		shouldSync = true;
	}

	private void OnSizeChanged(double value)
	{
		if (shouldSync && _item != null)
		{
			Vector2 size = new((float)_sizeX.Value, (float)_sizeY.Value);
			_item.TargetNode.Set(Control.PropertyName.Size, size);
			_item.Frame?.Set(Control.PropertyName.Size, size);
		}
	}

	private void OnPivotChanged(double value)
	{
		if (shouldSync && _item != null)
		{
			Vector2 pivot = new((float)_pivotX.Value, (float)_pivotY.Value);
			_item.TargetNode.Set(Control.PropertyName.PivotOffset, pivot);
			_item.Frame?.Set(Control.PropertyName.PivotOffset, pivot);

			var size = (Vector2)_item.TargetNode.Get(Control.PropertyName.Size);
			var pivotSize = Math.Min(size.X, size.Y) / 8;
			_item.Frame.pivot.Size = new Vector2(pivotSize, pivotSize).Max(32).Min(16);
			_item.Frame.pivot.Position = pivot - _item.Frame.pivot.Size / 2;
		}
	}

	private void OnPositionChanged(double value)
	{
		if (shouldSync && _item != null)
		{
			Vector2 pos = new((float)_positionX.Value, (float)_positionY.Value);
			_item.TargetNode.Set(Control.PropertyName.Position, pos);
			_item.Frame?.Set(Control.PropertyName.GlobalPosition, _item.TargetNode.Get(Control.PropertyName.GlobalPosition));
		}
	}

	private void OnRotationChanged(double value)
	{
		if (shouldSync && _item != null)
		{
			float rot = (float)Mathf.DegToRad(value);
			_item.TargetNode.Set(Control.PropertyName.Rotation, rot);
			_item.Frame?.Set(Control.PropertyName.Rotation, rot);
		}
	}

	private void OnScaleChanged(double value)
	{
		if (shouldSync && _item != null)
		{
			Vector2 scale = new((float)_scaleX.Value, (float)_scaleY.Value);
			_item.TargetNode.Set(Control.PropertyName.Scale, scale);
			_item.Frame?.Set(Control.PropertyName.Scale, scale);
		}
	}

	public void Reset()
	{
		shouldSync = false;

		_sizeContainer.Visible = true;
		_sizeX.Value = default(double);
		_sizeY.Value = default(double);

		_pivotContainer.Visible = true;
		_pivotX.Value = default(double);
		_pivotY.Value = default(double);

		_positionX.Value = default(double);
		_positionY.Value = default(double);

		_rotation.Value = default(double);

		_scaleX.Value = default(double);
		_scaleY.Value = default(double);

		shouldSync = true;
	}

	private void OnFoldingChanged(bool is_folded)
	{
		CustomMinimumSize = new Vector2(0, 0);
		if (is_folded) OnFolding?.Invoke();
	}
}
