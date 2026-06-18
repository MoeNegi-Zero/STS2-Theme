using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using System.Data.SqlTypes;
using System.IO;
using System.Reflection;

[ModInitializer(nameof(Initialize))]
public class MainFile
{
	public const string ModId = "STS2_Theme";

	public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
		new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

	public static void Initialize()
	{
		Harmony harmony = new(ModId);
		ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
		harmony.PatchAll();
	}
}
