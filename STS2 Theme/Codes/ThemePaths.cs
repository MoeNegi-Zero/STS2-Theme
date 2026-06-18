using Godot;
using System;
using System.IO;
using System.Reflection;

public static class ThemePaths
{
	public static readonly string ModDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

	public static readonly string ThemesDirectory = Path.Combine(Path.GetDirectoryName(OS.GetExecutablePath()), "Themes");

	public static string? ResolvePath(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return null;

		if (path.StartsWith("res://"))
			return path.Replace('\\', '/');

		if (path.StartsWith("user://"))
			return path.Replace('\\', '/');

		if (Path.IsPathRooted(path))
			return path;

		return $"res://Themes/{path.Replace('\\', '/')}";
	}

	public static void EnsureFilePathExists(string filePath)
	{
		var dir = Path.GetDirectoryName(filePath);

		if (!string.IsNullOrEmpty(dir))
		{
			Directory.CreateDirectory(dir);
		}
	}

	public static string GetAbosolutePath(string path)
	{
		if (path.StartsWith("res://")) path = path.Replace("res://", OS.GetExecutablePath().GetBaseDir()+"/");

		return path;
	}
}
