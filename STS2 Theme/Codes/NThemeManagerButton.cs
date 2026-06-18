using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using System.IO;

public partial class NThemeManagerButton : Control
{
	private static readonly StringName _v = new StringName("v");

	private ShaderMaterial _hsv;

	private TextureButton _icon;

	private Tween? _buttonTween;

	public override void _Ready()
	{
		_icon = GetNode<TextureButton>("Icon");
		_hsv = (ShaderMaterial)_icon.Material;
		_icon.Connect(TextureButton.SignalName.MouseEntered, Callable.From(OnHovered));
		_icon.Connect(TextureButton.SignalName.MouseExited, Callable.From(OnUnhovered));
		_icon.Connect(TextureButton.SignalName.ButtonDown, Callable.From(OnButtonDown));
	}

	protected void OnHovered()
	{
		_hsv.SetShaderParameter(_v, 1.2f);
		_icon.RotationDegrees = 5f;
	}

	protected void OnUnhovered()
	{
		_hsv.SetShaderParameter(_v, 1f);
		_icon.RotationDegrees = 0f;
	}

	protected void OnButtonDown()
	{
		//Log.Info("tscnPath = " + Path.Combine(ThemePaths.ThemesDirectory, "theme_manager.tscn"));
		//PackedScene testScene = ResourceLoader.Load<PackedScene>(Path.Combine(ThemePaths.ThemesDirectory, "theme_manager.tscn"));
		//NGame.Instance.RootSceneContainer.AddChild(testScene.Instantiate());
		var node = NGame.Instance.GetNodeOrNull<NThemeManager>("%ThemeManager");


		if (node != null)
		{
			node.MoveToFront();
			return;
		}
		

		NThemeManager themeManager = PreloadManager.Cache.GetScene("res://STS2 Theme/Scenes/theme_manager.tscn").Instantiate<NThemeManager>();
		themeManager.Name = "ThemeManager";
		themeManager.UniqueNameInOwner = true;
		NGame.Instance.AddChild(themeManager);
	}

	public void HideButton()
	{
		_buttonTween?.Kill();
		_buttonTween = CreateTween();
		_buttonTween.TweenProperty(this, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	public void ShowButton()
	{
		_buttonTween?.Kill();
		_buttonTween = CreateTween();
		_buttonTween.TweenProperty(this, "modulate:a", 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}
}
