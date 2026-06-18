using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;

public partial class NThemeCard : Control
{
	[Export]public string ThemeTitle { get; private set; }
	public string Author { get; private set; }
	[Export]public string ThemePath { get; private set; }
	public string PreviewThemePath { get; private set; }
	public string PreviewTexturePath { get; private set; }
	[Export]public Texture2D PreviewTexture { get; private set; }
	public string Description { get; private set; }
	public string Version { get; private set; }
	public string ResourceCfgPath { get; private set; }

	[Signal] public delegate void CardClickedEventHandler(NThemeCard card);
	[Signal] public delegate void HoverStartedEventHandler(NThemeCard card);
	[Signal] public delegate void HoverEndedEventHandler(NThemeCard card);
	
	private TextureRect _preview;
	private Label _title;
	private PanelContainer _highlight;

	public override void _Ready()
	{
		// 获取 UI 控件
		_preview = GetNode<TextureRect>("%Preview");
		_title = GetNode<Label>("%Title");
		_highlight = GetNodeOrNull<PanelContainer>("%Highlight");

		// 设置初始值
		if (PreviewTexture != null)
		{
			_preview.Texture = PreviewTexture;
		}
		_title.Text = ThemeTitle;

		MouseEntered += () => EmitSignal(SignalName.HoverStarted, this);
		MouseExited += () => EmitSignal(SignalName.HoverEnded, this);
	}

	public void LoadFromCfg(string cfgPath)
	{
		if (!File.Exists(cfgPath))
			return;

		var lines = File.ReadAllLines(cfgPath);
		var dict = new Dictionary<string, string>();
		foreach (var line in lines)
		{
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("["))
				continue;

			var kv = line.Split('=', 2);
			if (kv.Length == 2)
				dict[kv[0].Trim()] = kv[1].Trim();
		}

		dict.TryGetValue("Title", out var title);
		dict.TryGetValue("Author", out var author);
		dict.TryGetValue("ThemePath", out var themePath);
		dict.TryGetValue("PreviewThemePath", out var previewScenePath);
		dict.TryGetValue("PreviewTexture", out var previewTexturePath);
		dict.TryGetValue("Description", out var desc);
		dict.TryGetValue("Version", out var ver);
		dict.TryGetValue("ResourceCfgPath", out var resourceCfgPath);

		ThemeTitle = title ?? "Unknown Theme";
		Author = author ?? "Unknown";
		ThemePath = themePath is null ? null : ThemePaths.ResolvePath(themePath);
		PreviewThemePath = previewScenePath is null ? null : ThemePaths.ResolvePath(previewScenePath);
		PreviewTexturePath = previewTexturePath;
		Description = desc ?? "";
		Version = ver ?? "";
		ResourceCfgPath = resourceCfgPath is null ? null : resourceCfgPath;

		if (!string.IsNullOrEmpty(PreviewTexturePath))
		{
			var image = new Image();
			var path = $"{ThemePaths.ThemesDirectory}/{previewTexturePath}".Replace('\\', '/');
			var err = image.Load(path);
			if (err != Error.Ok)
			{
				Log.Error($"[>>>STS2 Theme]Failed to load preview image: {path}");
				return;
			}

			var tex = ImageTexture.CreateFromImage(image);

			PreviewTexture = tex;
		}
		else
		{
			PreviewTexture = PreloadManager.Cache.GetTexture2D("res://STS2 Theme/Images/ThemeCards/missing.png");
		}
	}

	/// <summary>设置缩略图</summary>
	public void SetThumbnail(Texture2D texture)
	{
		PreviewTexture = texture;
		if (_preview != null)
			_preview.Texture = texture;
	}

	/// <summary>设置标题</summary>
	public void SetTitle(string title)
	{
		ThemeTitle = title;
		if (_title != null)
			_title.Text = title;
	}

	/// <summary>设置Theme路径</summary>
	public void SetThemePath(string themePath)
	{
		ThemePath = themePath;
	}

	/// <summary>选中高亮</summary>
	public void Select()
	{
		if (_highlight != null)
			_highlight.Visible = true;
	}

	/// <summary>取消选中高亮</summary>
	public void Deselect()
	{
		if (_highlight != null)
			_highlight.Visible = false;
	}

	/// <summary>设置预览/// </summary>
	public void SetPreview(ViewportTexture texture)
	{
		_preview.Texture = texture;
	}

	/// <summary>重置预览/// </summary>
	public void ResetPreview()
	{
		_preview.Texture = PreviewTexture;
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
		{
			EmitSignal("CardClicked", this);
		}
	}
}
