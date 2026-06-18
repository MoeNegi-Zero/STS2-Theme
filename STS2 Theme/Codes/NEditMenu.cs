using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

public partial class NEditMenu : Control
{
	private readonly Dictionary<string, EditableItem> _items = new();
	private EditableItem? _selectedItem;

	private NPropertyPanel propertyPanel;
	private VBoxContainer mainMenuNodesPanel;
	private VBoxContainer mainMenuBgNodesPanel;
	private CanvasLayer frameContainer;
	private NNodesFolder mainMenuFolder;
	private NNodesFolder mainMenuBgFolder;
	public TextureButton preview;
	private PopupMenu menu;
	private FileDialog fileDialog;
	private string fileType;
	private Node applyNode;

	private Button save;
	public LineEdit title;
	private LineEdit author;
	public string previewImagePath;

	private CheckBox HideFrames;
	private CheckBox ResetMainMenu;


	private SceneRegistry mainMenuRegistry;

	public override void _Ready()
	{
		propertyPanel = GetNode<NPropertyPanel>("%PropertyPanel");
		mainMenuNodesPanel = GetNode<VBoxContainer>("%MainMenuNodes");
		mainMenuBgNodesPanel = GetNode<VBoxContainer>("%MainMenuBgNodes");
		frameContainer = GetNode<CanvasLayer>("%FrameContainer");
		mainMenuFolder = GetNode<NNodesFolder>("%MainMenuNodesFolder");
		mainMenuBgFolder = GetNode<NNodesFolder>("%MainMenuBgNodesFolder");
		preview = GetNode<TextureButton>("%Preview");
		fileDialog = GetNode<FileDialog>("%FileDialog");
		title = GetNode<LineEdit>("%Title");
		author = GetNode<LineEdit>("%Author");
		save = GetNode<Button>("%Save");
		HideFrames = GetNode<CheckBox>("%HideFrames");
		ResetMainMenu = GetNode<CheckBox>("%ResetMainMenu");

		fileDialog.FileSelected += SelectFile;
		title.TextChanged += OnTitleTextChanged;

		preview.Pressed += OnChoosePreviewImgButtonPressed;

		save.Pressed += OnSaveButtonPressed;
		GetNode<Button>("%Exit").Pressed += OnExitButtonPressed;

		HideFrames.Toggled += OnHideFramesToggle;

		menu = new PopupMenu();
		AddChild(menu);

		menu.AddItem("Add TextureRect", 0);
		menu.AddItem("Add VideoPlayer", 1);
		menu.AddItem("Delete Node", 2);

		menu.IdPressed += OnMenuItemPressed;

		mainMenuFolder.OnRightClicked += OpenRightClickMenu;
		mainMenuBgFolder.OnRightClicked += OpenRightClickMenu;

		RegisterOriginalNodes();

		if(title.Text == null || title.Text == "") save.Disabled = true;
	}

	private void RegisterOriginalNodes()
	{
		var MainMenu = NMainMenuPatch.nMainMenu;

		HashSet<Node> whitelist = [
			/*MainMenu.GetNode("%MainMenuTextButtons"),
			NodeUtil.FindFirstNodeOfType<NOpenProfileScreenButton>(MainMenu),
			NodeUtil.FindFirstNodeOfType<NPatchNotesButton>(MainMenu),
			MainMenu.GetNode("%ModdedWarning"),
			NodeUtil.FindFirstNodeOfType<NFpsVisualizer>(MainMenu),
			MainMenu.GetNode("%ReleaseInfo"),
			MainMenu.GetNode("%ModWarningContainer")*/
		];

		HashSet<Node> blacklist = [
			MainMenu.GetNodeOrNull("%ButtonReticleLeft"),
			MainMenu.GetNodeOrNull("%ButtonReticleRight"),
			MainMenu.GetNodeOrNull("%BlurBackstop"),
			MainMenu.GetNodeOrNull("%PatchNotesScreen"),
			MainMenu.GetNodeOrNull("%Submenus"),
			MainMenu.GetNodeOrNull("%MainMenuBg")
		];

		foreach (var node in MainMenu.GetChildren())
		{
			if ((whitelist.Contains(node)|| node is Node2D || node is Control) && !blacklist.Contains(node))
			{
				node.Set(Control.PropertyName.Visible, true);
				var frame = DrawFrame(node);
				frameContainer.AddChild(frame);
				RegisterNode(node, frame, mainMenuNodesPanel);
			}
			else if (node is NMainMenuBg)
			{
				mainMenuNodesPanel.GetNode<FoldableContainer>("MainMenuBgNodesFolder").MoveToFront();
			}
			else
			{
				mainMenuNodesPanel.AddChild(CreateUnSelectableNode(node));
			}
		}

		/*
		Node[] list = [
			MainMenu.GetNode("%MainMenuTextButtons"),
			NodeUtil.FindFirstNodeOfType<NOpenProfileScreenButton>(MainMenu),
			NodeUtil.FindFirstNodeOfType<NPatchNotesButton>(MainMenu),
			MainMenu.GetNode("%ModdedWarning"),
			NodeUtil.FindFirstNodeOfType<NFpsVisualizer>(MainMenu),
			MainMenu.GetNode("%ReleaseInfo"),
			MainMenu.GetNode("%ModWarningContainer"),
		];

		foreach (var node in list)
		{
			if (node != null)
			{
				node.Set(Control.PropertyName.Visible, true);
				var frame = DrawFrame(node);
				frameContainer.AddChild(frame);
				RegisterNode(node, frame);
			}
		}*/

		var MainMenuBg = NMainMenuPatch.nMainMenuBg;
		var logo = MainMenuBg.GetNode("%Logo");
		foreach (var node in MainMenuBg.GetNode("BgContainer").GetChildren())
		{

			if (node == logo.GetParent())
			{
				RegisterNode(logo, null, mainMenuBgNodesPanel);
			}
			else if ((whitelist.Contains(node)|| node is Node2D || node is Control)&& !blacklist.Contains(node))
			{
				node.Set(Control.PropertyName.Visible, true);
				var frame = DrawFrame(node);
				frameContainer.AddChild(frame);
				RegisterNode(node, frame, mainMenuBgNodesPanel);
			}
			else
			{
				mainMenuBgNodesPanel.AddChild(CreateUnSelectableNode(node));
			}
		}
	}

	private EditableItem RegisterNode(Node node, NEditableFrame? frame, Node panel, bool couldRemove = false)
	{
		var selectableNode = CreateSelectableNode(node, frame);
		var id = NodeUtil.BuildID(node);
		Log.Info($">>>[STS2 Theme]Node ID = {id}");
		var item = new EditableItem
		{
			Name = node.Name,
			TargetNode = node,
			Frame = frame,
			SelectableNode = selectableNode
		};

		_items[id] = item;

		if (frame != null)
			frame.ItemName = id;

		selectableNode.ItemName = id;

		if (node is NRuntimeTextureRect || node is VideoStreamPlayer)
		{
			selectableNode.couldSelectFile = true;
			selectableNode.OnSelectFileButtonPressed += OnChooseFileButtonPressed;
		}

		panel.AddChild(selectableNode);

		// 连接信号
		if (frame != null)
		{
			frame.FrameSelected += OnFrameSelected;
			frame.OnDragging += OnFrameDragging;
			frame.AfterDragging += AfterFrameDragging;
		}

		selectableNode.NodeSelected += OnNodeSelected;

		return item;
	}

	private NSelectableNode CreateSelectableNode(Node node, NEditableFrame? frame, bool couldRemove = false)
	{
		var selectableNode = ResourceLoader.Load<PackedScene>("res://STS2 Theme/Scenes/selectable_node.tscn").Instantiate<NSelectableNode>();
		selectableNode._node = node;
		selectableNode._frame = frame;
		selectableNode._title = node.Name;
		selectableNode.couldRemove = couldRemove;
		selectableNode.BeforeQueueFree += BeforeSelectableNodeQueueFree;
		selectableNode.OnVisibleButtonPressed += OnSelectableNodeVisibleButtonPressed;

		return selectableNode;
	}

	private static NUnSelectableNode CreateUnSelectableNode(Node node)
	{
		var unSelectableNode = ResourceLoader.Load<PackedScene>("res://STS2 Theme/Scenes/unselectable_node.tscn").Instantiate<NUnSelectableNode>();
		unSelectableNode._title = node.Name;
		return unSelectableNode;
	}

	private NEditableFrame DrawFrame(Node node)
	{
		if (node is not Control) return null;
		var frame = new NEditableFrame();
		frame.Initialize(node.Name + "Frame", node);
		frame.Visible = false;
		NodeUtil.SyncFromNode(node, frame);
		frame.FrameSelected += OnFrameSelected;
		return frame;
	}

	private void OnFrameSelected(string itemName)
	{
		SelectItem(itemName);
	}

	private void OnNodeSelected(string itemName)
	{
		SelectItem(itemName);
	}

	private void SelectItem(string itemName)
	{
		if (!_items.TryGetValue(itemName, out var item))
			return;

		if (_selectedItem == item)
			return;

		// 取消上一个
		if (_selectedItem != null)
		{
			_selectedItem.Frame?.Hide();
			_selectedItem.Frame?.Deselect();
			_selectedItem.SelectableNode.Deselect();
		}

		_selectedItem = item;

		_selectedItem.Frame?.Show();
		_selectedItem.Frame?.Select();
		_selectedItem.SelectableNode.Select();

		propertyPanel.ShowProperties(_selectedItem);
	}

	public void OnFrameDragging(NEditableFrame frame)
	{
		if (!_items.TryGetValue(frame.ItemName, out var item))
			return;

		item.TargetNode.Set(Control.PropertyName.GlobalPosition, frame.GlobalPosition);
		propertyPanel.ShowProperties(item);
	}

	public void AfterFrameDragging(NEditableFrame frame)
	{
		propertyPanel.shouldSync = true;
	}

	private void BeforeSelectableNodeQueueFree(string name, NSelectableNode node)
	{
		var key = NodeUtil.BuildKey(node._node);

		mainMenuRegistry.Remove(key);
	}

	private void OnSelectableNodeVisibleButtonPressed(NSelectableNode node)
	{
		if (_selectedItem.SelectableNode == null)
			return;

		if (_selectedItem.SelectableNode == node && _selectedItem.Frame!= null)
		{
			_selectedItem.Frame.Visible = (bool)_selectedItem.TargetNode.Get("visible");
		}
	}


	private void OpenRightClickMenu(Node root, VBoxContainer nodePanel)
	{
		menu.Position = (Vector2I)GetGlobalMousePosition();
		menu.SetMeta("Root", root);
		menu.SetMeta("NodePanel", nodePanel);
		
		if (_selectedItem == null)
		{
			menu.SetItemDisabled(2, true);
		}
		else
		{
			menu.SetItemDisabled(2, false);
		}
		menu.Popup();
	}

	private void OnMenuItemPressed(long id)
	{
		Node root = (Node)menu.GetMeta("Root");
		VBoxContainer nodePanel = (VBoxContainer)menu.GetMeta("NodePanel");

		switch (id)
		{
			case 0:
				var runtimeTextureRect = new NRuntimeTextureRect();
				runtimeTextureRect.Size = new Vector2(256, 256);
				runtimeTextureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
				AddNode(runtimeTextureRect, root ,nodePanel);
				break;
			case 1:
				var node = new VideoStreamPlayer();
				node.Loop = true;
				node.Autoplay = true;
				node.Expand = true;
				AddNode(node, root, nodePanel);
				break;
			case 2: 
				DeleteNode();
				break;
		}
	}

	private void AddNode(Node node,  Node root, VBoxContainer nodePanel)
	{
		root.AddChild(node);

		if (root.GetParent() == NMainMenuPatch.nMainMenuBg) root = NMainMenuPatch.nMainMenuBg;
		node.Owner = root;

		NEditableFrame frame = DrawFrame(node);
		frameContainer.AddChild(frame);
		RegisterNode(node, frame, nodePanel, true);
	}

	private void DeleteNode()
	{
		var id = NodeUtil.BuildID(_selectedItem.TargetNode);
		if (_selectedItem.Frame != null) _selectedItem.Frame.QueueFree();
		_selectedItem.TargetNode.QueueFree();
		_selectedItem.SelectableNode.QueueFree();
		propertyPanel.Reset();
		_items.Remove(id);
		_selectedItem = null;
	}

	private void OnChoosePreviewImgButtonPressed()
	{
		fileType = "PreviewImage";
		applyNode = preview;
		fileDialog.PopupCentered();
	}

	private void OnChooseFileButtonPressed(Node node)
	{
		if (node is TextureRect)
		{
			fileType = "TextureImage";
		}

		if (node is VideoStreamPlayer)
		{
			fileType = "VideoStream";
		}

		applyNode = node;
		fileDialog.PopupCentered();
	}

	private void SelectFile(string path)
	{
		switch (fileType)
		{
			case "PreviewImage":
				if (path.EndsWith(".png"))
				{
					var image = new Image();
					var err = image.Load(path);
					if (err != Error.Ok)
					{
						Log.Error($"[>>>STS2 Theme]Failed to load preview image: {path}");
						return;
					}

					var tex = ImageTexture.CreateFromImage(image);
					preview.TextureNormal = tex;

					previewImagePath = path;
				}
				break;

			case "TextureImage":
				if (path.EndsWith(".png") && applyNode is TextureRect textureRect)
				{
					var image = new Image();
					var err = image.Load(path);
					if (err != Error.Ok)
					{
						Log.Error($"[>>>STS2 Theme]Failed to load TextureImage: {path}");
						return;
					}

					var tex = ImageTexture.CreateFromImage(image);
					textureRect.Texture = tex;

					ResourceHandler._map[NodeUtil.BuildID(applyNode)] = path;
				}
				break;

			case "VideoStream":
				if (path.EndsWith(".ogv") && applyNode is VideoStreamPlayer streamPlayer)
				{
					var stream = new VideoStreamTheora();
					stream.File = path;

					streamPlayer.Stream = stream;
					streamPlayer.Play();
				}
				break;

		}
	}

	private void OnTitleTextChanged(string value)
	{
		if (title.Text != null && title.Text != "")
			save.Disabled = false;
		else
			save.Disabled = true;
	}

	private void OnSaveButtonPressed()
	{
		var bg = NMainMenuPatch.nMainMenuBg;
		var themeTitle = title.Text;

		var path = ProjectSettings.GlobalizePath($"res://Themes/{themeTitle}/{themeTitle}.rsrc");
		ThemePaths.EnsureFilePathExists(ProjectSettings.GlobalizePath(path));
		//Directory.CreateDirectory(Path.GetDirectoryName(path));

		// 写回文件
		StreamWriter writer = new StreamWriter(path, false);
		writer.WriteLine("[Resource]");

		var list = NMainMenuPatch.nMainMenu.GetChildren() + NMainMenuPatch.nMainMenuBg.GetNode("BgContainer").GetChildren();

		ThemePaths.EnsureFilePathExists(ProjectSettings.GlobalizePath($"res://Themes/{themeTitle}/Images/test.png"));
		ThemePaths.EnsureFilePathExists(ProjectSettings.GlobalizePath($"res://Themes/{themeTitle}/Videos/test.ovg"));

		Dictionary<string,string> synced = [];

		foreach (Node node in list)
		{
			if (node is NRuntimeTextureRect runtimeTextureRect)
			{
				var id = NodeUtil.BuildID(node);
				if (!synced.Keys.Contains(ResourceHandler._map[id]))
				{
					string imgpath = ResourceHandler._map[id];
					if (imgpath.Replace("/","\\").StartsWith(Path.GetFullPath(ProjectSettings.GlobalizePath($"res://Themes/{themeTitle}/Images/"))))
					{
						imgpath = $"res://Themes/{themeTitle}/Images/{Path.GetFileName(imgpath)}";
						Log.Info($"this is a file not cached by synced.NodeID = {id} path changes to {imgpath}");
					}
					else if (!ResourceHandler._map[id].StartsWith($"res://Themes/{themeTitle}/Images/"))
					{
						imgpath = $"res://Themes/{themeTitle}/Images/{id.GetHashCode()}.png";
						File.Copy(ThemePaths.GetAbosolutePath(ResourceHandler._map[id]), ThemePaths.GetAbosolutePath(imgpath), true);
						Log.Info($"this is a file not saved in ImagesFile.NodeiD = {id} path changes to {imgpath}");
					}

					synced[ResourceHandler._map[id]] = imgpath;
					ResourceHandler._map[id] = imgpath;
				}
				else
				{
					ResourceHandler._map[id] = synced[ResourceHandler._map[id]];
					Log.Info($"this is a file cached by synced.NodeID = {id} path changes to {ResourceHandler._map[id]}");
				}
				runtimeTextureRect.Texture = null;
				Log.Info($"key is {id}, value is {ResourceHandler._map[id]}");
				writer.WriteLine($"{id}={ResourceHandler._map[id]}");
				/*
				if (!ResourceHandler._map[id].StartsWith("res://"))
				{
					var imgpath = $"res://Themes/{themeTitle}/Images/{id.GetHashCode()}.png";
					File.Copy(ResourceHandler._map[id], ProjectSettings.GlobalizePath(imgpath), true);
					ResourceHandler._map[id] = imgpath;
				}
				runtimeTextureRect.Texture = null;
				writer.WriteLine($"{id}={ResourceHandler._map[id]}");*/
			}

			if (node is VideoStreamPlayer player)
			{
				var id = NodeUtil.BuildID(node);


				if (player.Stream.File.Replace("/","\\").StartsWith(Path.GetFullPath(ProjectSettings.GlobalizePath($"res://Themes/{themeTitle}/Videos/"))))
				{
					player.Stream.File = $"res://Themes/{themeTitle}/Videos/{Path.GetFileName(player.Stream.File)}";
					Log.Info($"this is a video file not cached by synced.NodeID = {id} path changes to {player.Stream.File}");
				}
				else if (!player.Stream.File.StartsWith($"res://Themes/{themeTitle}/Videos/"))
				{
					var videopath = $"res://Themes/{themeTitle}/Videos/{id.GetHashCode()}.ogv";
					File.Copy(ThemePaths.GetAbosolutePath(player.Stream.File), ThemePaths.GetAbosolutePath(videopath), true);
					player.Stream.File = videopath;
					Log.Info($"this is a video file not saved in VideosFile.NodeID = {id} path changes to {player.Stream.File}");
				}
			}
		}
		writer.Flush();
		writer.Close();

		path = $"res://Themes/{themeTitle}/Scenes/main_menu_bg.tscn";
		ThemePaths.EnsureFilePathExists(ProjectSettings.GlobalizePath(path));
		SaveScene(bg, path);

		var themePath = $"res://Themes/{themeTitle}/Scenes/main_menu.tscn";
		ThemePaths.EnsureFilePathExists(ProjectSettings.GlobalizePath(themePath));
		SaveScene(NMainMenuPatch.nMainMenu, themePath);

		path = ProjectSettings.GlobalizePath($"res://Themes/{themeTitle}/{themeTitle}.cfg");
		ThemePaths.EnsureFilePathExists(ProjectSettings.GlobalizePath(path));


		var rsrcPath = $"{themeTitle}/{themeTitle}.rsrc";
		string previewTexture = "";
		if (previewImagePath != null) previewTexture = $"{themeTitle}/preview.png";

		writer = new StreamWriter(path, false);
		writer.WriteLine($"[{themeTitle}]");
		writer.WriteLine($"Title={themeTitle}");
		writer.WriteLine($"Author={author.Text}");
		writer.WriteLine($"ThemePath={themeTitle}/Scenes/main_menu.tscn");
		writer.WriteLine($"PreviewThemePath=");
		writer.WriteLine($"PreviewTexture={previewTexture}");
		writer.WriteLine($"Description=");
		writer.WriteLine($"ResourceCfgPath={rsrcPath}");
		writer.WriteLine($"Version=");

		writer.Flush();
		writer.Close();


		TscnExtResourcePatcher.ReplaceNodeResourcePath($"Themes/{themeTitle}/Scenes/main_menu.tscn", "MainMenuBg", $"res://Themes/{themeTitle}/Scenes/main_menu_bg.tscn");

		ConfigFile.SetValue("DefaultThemePath", themePath);
		ConfigFile.SetValue("DefaultThemeResourceCfgPath", "Themes/" + rsrcPath);
		ConfigFile.Reload();

		if (previewImagePath != null && ThemePaths.GetAbosolutePath(previewImagePath)!=ThemePaths.GetAbosolutePath($"res://Themes/{themeTitle}/preview.png")) File.Copy(ThemePaths.GetAbosolutePath(previewImagePath), ThemePaths.GetAbosolutePath($"res://Themes/{themeTitle}/preview.png"), true);

		if(NMainMenuPatch.Cache.ContainsKey(themePath)) NMainMenuPatch.Cache.Remove(themePath);

		NGame.Instance.ReloadMainMenu();
		this.QueueFree();
	}

	public void SaveScene(Node root, string path)
	{
		var packed = new PackedScene();

		var result = packed.Pack(root);

		if (result != Error.Ok)
		{
			GD.PrintErr($"Pack failed: {result}");
			return;
		}

		var saveResult = ResourceSaver.Save(packed, path);

		if (saveResult != Error.Ok)
		{
			GD.PrintErr($"Save failed: {saveResult}");
			return;
		}

		GD.Print($"Saved temp scene to: {path}");
	}

	private void OnHideFramesToggle(bool toggle_on)
	{
		if (toggle_on) frameContainer.Visible = false;
		else frameContainer.Visible = true;
	}

	public void OnExitButtonPressed()
	{
		if (ResetMainMenu.ButtonPressed == true)
		{
			NGame.Instance.ReloadMainMenu();

		}
		this.QueueFree();
	}
}
