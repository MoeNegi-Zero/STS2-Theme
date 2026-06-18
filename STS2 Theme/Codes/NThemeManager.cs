using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using static Godot.OpenXRCompositionLayer;

public partial class NThemeManager : Control
{
	[Export] public PackedScene ThemeCardScene;

	private GridContainer _themeGrid;
	private List<NThemeCard> _allCards = new List<NThemeCard>();
	
	private LineEdit _searchBar;

	private TextureButton _closeButton;
	private Button _editModeButton;
	private Button _createButton;
	private Button _loadButton;

	private NThemeCard _currentSelectedCard;
	
	private SubViewport _previewViewport;
	private Node _currentPreviewInstance;
	private Dictionary<string, PackedScene> _sceneCache = new();

	private Button create;
	private Button import;
	private Button upload;

	public static string? CurrentThemePath { get; set; }

	public override void _Ready()
	{
		_themeGrid = GetNode<GridContainer>("%ThemeGrid");
		_themeGrid.Columns = 6; // 横向6列

		_searchBar = GetNode<LineEdit>("%TextArea");
		_searchBar.TextChanged += OnSearchTextChanged;
		_searchBar.PlaceholderText = LocManager.Instance.GetTable("STS2 Theme").GetRawText("THEMEMANAGER_SEARCH");

		_previewViewport = GetNode<SubViewport>("%SubViewport");

		_closeButton = GetNode<TextureButton>("%Close");
		_closeButton.Connect(TextureButton.SignalName.ButtonDown, Callable.From(Close));

		_editModeButton = GetNode<Button>("%Edit");
		_editModeButton.Connect(Button.SignalName.ButtonDown, Callable.From(EnterEditMode));
		_editModeButton.Text = LocManager.Instance.GetTable("STS2 Theme").GetRawText("THEMEMANAGER_EDIT");
		_editModeButton.Disabled = true;

		create = GetNode<Button>("%Create");
		create.Pressed += OnCreateButtonPressed;
		create.Text = LocManager.Instance.GetTable("STS2 Theme").GetRawText("THEMEMANAGER_CREATE");

		import = GetNode<Button>("%Import");
		import.Pressed += OnImportButtonPressed;
		import.Text = LocManager.Instance.GetTable("STS2 Theme").GetRawText("THEMEMANAGER_IMPORT");

		upload = GetNode<Button>("%Upload");
		upload.Text = LocManager.Instance.GetTable("STS2 Theme").GetRawText("THEMEMANAGER_UPLOAD");

		if(LocManager.Instance.Language == "zhs") 
		GetNode<LinkButton>("%Title").Uri = "https://space.bilibili.com/7350797?spm_id_from=333.1007.0.0";

		AddVaniila();
		LoadThemes(ThemePaths.ThemesDirectory);
	}

	private void AddVaniila()
	{
		var card = ThemeCardScene.Instantiate<NThemeCard>();
		card.SetTitle("Vanilla");
		card.SetThumbnail(ResourceLoader.Load<Texture2D>("res://STS2 Theme/Images/ThemeCards/main_menu_logo.png"));
		card.SetThemePath("res://scenes/screens/main_menu.tscn");
		card.CardClicked += OnCardClicked;
		_themeGrid.AddChild(card);
		_allCards.Add(card);
	}

	private void LoadThemes(string folder)
	{
		if (!Directory.Exists(folder))
			return;

		foreach (var config in Directory.GetFiles(folder, "*.cfg",SearchOption.AllDirectories))
		{
			Log.Info("Loading Themes");
			var card = ThemeCardScene.Instantiate<NThemeCard>();
			card.LoadFromCfg(config);
			card.CardClicked += OnCardClicked;
			card.HoverStarted += OnCardHoverStarted;
			card.HoverEnded += OnCardHoverEnded;

			_themeGrid.AddChild(card);
			_allCards.Add(card);
		}

		/*
		foreach (NThemeCard nTheme in _themeGrid.GetChildren())
		{
			nTheme.CardClicked += OnCardClicked;
			nTheme.HoverStarted += OnCardHoverStarted;
			nTheme.HoverEnded += OnCardHoverEnded;
		}*/
	}

	private void OnSearchTextChanged(string text)
	{
		text = text.ToLower();
		foreach (var card in _allCards)
		{
			var title = card.ThemeTitle.ToLower(); // ThemeCard 里存标题
			card.Visible = title.Contains(text);
		}
	}

	private void OnCardHoverStarted(NThemeCard card)
	{
		LoadPreviewScene(card);
	}

	private void OnCardHoverEnded(NThemeCard card)
	{
		// 清理实例化的节点
		if (_currentPreviewInstance != null)
		{
			_currentPreviewInstance.QueueFree();
			_currentPreviewInstance = null;
		}

		// 还原静态预览图
		card.ResetPreview();
	}

	private void OnCardClicked(NThemeCard card)
	{
		if (_currentSelectedCard != null)
			_currentSelectedCard.Deselect();

		_currentSelectedCard = card;
		_currentSelectedCard.Select();
		
		CurrentThemePath = _currentSelectedCard.ThemePath;
		ConfigFile.SetValue("DefaultThemePath", CurrentThemePath);
		ConfigFile.SetValue("DefaultThemeResourceCfgPath", "Themes/"+_currentSelectedCard.ResourceCfgPath);
		_editModeButton.Disabled = false;
		NGame.Instance.ReloadMainMenu();
	}

	private void LoadPreviewScene(NThemeCard card)
	{
		if (card.PreviewThemePath == null)
		{
			return;
		}

		// 清理上一轮实例
		if (_currentPreviewInstance != null)
		{
			_currentPreviewInstance.QueueFree();
			_currentPreviewInstance = null;
		}

		// 获取 PackedScene（缓存）
		if (!_sceneCache.TryGetValue(card.ThemePath, out var packed))
		{
			packed = ResourceLoader.Load<PackedScene>(card.PreviewThemePath);
			if (packed == null)
				return;
			_sceneCache[card.ThemePath] = packed;
		}

		// 实例化
		_currentPreviewInstance = packed.Instantiate();

		// 移除交互节点
		//RemoveInteractiveNodes(_currentPreviewInstance);

		// 添加到 SubViewport
		_previewViewport.AddChild(_currentPreviewInstance);

		// 将 SubViewport 输出纹理赋值给 ThemeCard
		card.SetPreview(_previewViewport.GetTexture());
	}

	protected void OnCreateButtonPressed()
	{
		ConfigFile.SetValue("DefaultThemePath", null);
		ConfigFile.SetValue("DefaultThemeResourceCfgPath", null);
		NGame.Instance.ReloadMainMenu();
		EnterEditMode();
	}


	protected void EnterEditMode()
	{
		var node = NGame.Instance.GetNodeOrNull<NEditMenu>("%EidtMenu");

		if (node != null)
		{
			node.MoveToFront();
			this.QueueFree();
			return;
		}

		var editMenu = ResourceLoader.Load<PackedScene>("res://STS2 Theme/Scenes/edit_menu.tscn").Instantiate<NEditMenu>();
		editMenu.Name = "EditMenu";
		editMenu.UniqueNameInOwner = true;
		NGame.Instance.AddChild(editMenu);
		if (_currentSelectedCard != null)
		{
			editMenu.previewImagePath = "res://Themes/" + _currentSelectedCard.PreviewTexturePath;
			editMenu.preview.TextureNormal = _currentSelectedCard.PreviewTexture;
			editMenu.title.Text = _currentSelectedCard.ThemeTitle;
		}
		this.QueueFree();
	}

	protected void OnImportButtonPressed()
	{
		var dialog = new FileDialog();

		dialog.FileMode = FileDialog.FileModeEnum.OpenFile;
		dialog.Access = FileDialog.AccessEnum.Filesystem;

		dialog.Filters = new[] { "*.zip" };
		dialog.Title = "Import Theme Package";
		dialog.Access = FileDialog.AccessEnum.Filesystem;
		dialog.UseNativeDialog = true;

		dialog.FileSelected += path =>
		{
			ImportZip(path);
			dialog.QueueFree();
		};

		dialog.Canceled += () =>
		{
			dialog.QueueFree();
		};

		AddChild(dialog);
		dialog.PopupCentered();
	}

	protected void ImportZip(string zipPath)
	{
		var themeName = Path.GetFileNameWithoutExtension(zipPath);

		var targetDir = ProjectSettings.GlobalizePath($"res://Themes/");

		Directory.CreateDirectory(targetDir);

		ZipFile.ExtractToDirectory(zipPath, targetDir, true);

		foreach (var config in Directory.GetFiles($"{targetDir}/{themeName}", "*.cfg", SearchOption.AllDirectories))
		{
			var card = ThemeCardScene.Instantiate<NThemeCard>();
			card.LoadFromCfg(config);
			card.CardClicked += OnCardClicked;
			card.HoverStarted += OnCardHoverStarted;
			card.HoverEnded += OnCardHoverEnded;

			_themeGrid.AddChild(card);
			_allCards.Add(card);
		}
	}

	public void Reload()
	{
		_currentSelectedCard = null;
		if (_currentPreviewInstance != null)
		{
			_currentPreviewInstance.QueueFree();
			_currentPreviewInstance = null;
		}
		foreach (NThemeCard card in _themeGrid.GetChildren())
		{
			card.QueueFree();
		}
		_allCards = [];
		_sceneCache = null;

		AddVaniila();
		LoadThemes(ThemePaths.ThemesDirectory);
	}

	protected void Close()
	{
		this.QueueFree();
	}
}
