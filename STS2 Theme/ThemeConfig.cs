using System;
using System.Collections.Generic;
using System.IO;

public class ThemeConfig
{
	// Theme信息
	public string? Title { get; set; }
	public string? Author { get; set; }
	public string? ThemePath { get; set; }
	public string? PreviewThemePath { get; set; }
	public string? PreviewTexturePath { get; set; }
	public string? Description { get; set; }
	public string? Version { get; set; }

	// 可同步节点列表
	public HashSet<string> MainMenuSyncList { get; private set; } = new();
	public HashSet<string> BgSyncList { get; private set; } = new();

	/// <summary>
	/// 从 cfg 文件加载 Theme 配置
	/// </summary>
	public void Load(string cfgPath)
	{
		if (!File.Exists(cfgPath))
			return;

		string? currentSection = null;
		foreach (var line in File.ReadAllLines(cfgPath))
		{
			string trimmed = line.Trim();
			if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(";") || trimmed.StartsWith("#"))
				continue;

			if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
			{
				currentSection = trimmed.Substring(1, trimmed.Length - 2);
				continue;
			}

			switch (currentSection)
			{
				case "Theme":
					{
						var kv = trimmed.Split('=', 2);
						if (kv.Length != 2) continue;
						string key = kv[0].Trim();
						string value = kv[1].Trim();
						switch (key)
						{
							case "Title": Title = value; break;
							case "Author": Author = value; break;
							case "ThemePath": ThemePath = value; break;
							case "PreviewThemePath": PreviewThemePath = value; break;
							case "PreviewTexture": PreviewTexturePath = value; break;
							case "Description": Description = value; break;
							case "Version": Version = value; break;
						}
						break;
					}
				case "MainMenuSyncList":
					MainMenuSyncList.Add(trimmed);
					break;
				case "BgSyncList":
					BgSyncList.Add(trimmed);
					break;
			}
		}
	}

	/// <summary>
	/// 保存 Theme 配置到 cfg 文件
	/// </summary>
	public void Save(string cfgPath)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(cfgPath) ?? "");

		using var writer = new StreamWriter(cfgPath, false);
		writer.WriteLine("[Theme]");
		writer.WriteLine($"Title={Title}");
		writer.WriteLine($"Author={Author}");
		writer.WriteLine($"ThemePath={ThemePath}");
		writer.WriteLine($"PreviewThemePath={PreviewThemePath}");
		writer.WriteLine($"PreviewTexture={PreviewTexturePath}");
		writer.WriteLine($"Description={Description}");
		writer.WriteLine($"Version={Version}");
		writer.WriteLine();

		writer.WriteLine("[MainMenuSyncList]");
		foreach (var item in MainMenuSyncList)
		{
			writer.WriteLine(item);
		}
		writer.WriteLine();

		writer.WriteLine("[BgSyncList]");
		foreach (var item in BgSyncList)
		{
			writer.WriteLine(item);
		}
	}

	/// <summary>
	/// 添加节点到 MainMenuSyncList
	/// </summary>
	public void AddToMainMenu(string nodeName)
	{
		MainMenuSyncList.Add(nodeName);
	}

	/// <summary>
	/// 添加节点到 BgSyncList
	/// </summary>
	public void AddToBg(string nodeName)
	{
		BgSyncList.Add(nodeName);
	}

	/// <summary>
	/// 从列表删除节点
	/// </summary>
	public void RemoveFromMainMenu(string nodeName)
	{
		MainMenuSyncList.Remove(nodeName);
	}

	public void RemoveFromBg(string nodeName)
	{
		BgSyncList.Remove(nodeName);
	}
}
