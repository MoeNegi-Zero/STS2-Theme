using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

[HarmonyPatch(typeof(NMainMenu))]
public static class NMainMenuPatch
{
    private static readonly FieldInfo OpenTimelineField = AccessTools.Field(typeof(NMainMenu), "_openTimeline");

    public static Dictionary<string, PackedScene> Cache = [];

    private static NThemeManagerButton themeManagerButton;

    public static NMainMenu nMainMenu;
    public static NMainMenuBg nMainMenuBg;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(NMainMenu.Create))]
    static bool PatchNmainMenuCreate(ref NMainMenu __result, bool openTimeline)
    {
        /*if (NThemeManager.CurrentThemePath != null)
        {
            if (Cache.ContainsKey(NThemeManager.CurrentThemePath))
            {
                Cache.TryGetValue(ConfigFile.GetValue("DefaultThemePath"), out PackedScene cachedMainMenu);
                __result = cachedMainMenu.Instantiate<NMainMenu>();
            }
            else
            {
                __result = ResourceLoader.Load<PackedScene>(NThemeManager.CurrentThemePath).Instantiate<NMainMenu>(PackedScene.GenEditState.Disabled);
            }
            OpenTimelineField.SetValue(__result, openTimeline);
            return false;
        }*/
        ResourceHandler._map.Clear();

        if (ConfigFile.GetValue("DefaultThemePath") != null && ConfigFile.GetValue("DefaultThemePath") != "")
        {
            var cfg = ConfigFile.GetValue("DefaultThemeResourceCfgPath");
            if (cfg != null)
            {
                ResourceHandler.LoadFromCfg(cfg);
            }

            if (Cache.ContainsKey(ConfigFile.GetValue("DefaultThemePath")))
            {
                Cache.TryGetValue(ConfigFile.GetValue("DefaultThemePath"), out PackedScene cachedMainMenu);
                __result = cachedMainMenu.Instantiate<NMainMenu>();
            }
            else
            {
                __result = ResourceLoader.Load<PackedScene>(ConfigFile.GetValue("DefaultThemePath"),null,ResourceLoader.CacheMode.Ignore).Instantiate<NMainMenu>(PackedScene.GenEditState.Disabled);
            }
            OpenTimelineField.SetValue(__result, openTimeline);
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(NMainMenu.EnableBackstop))]
    static void PatchNMainMenuEnableBackstop()
    {
        //themeManagerButton.HideButton();
        themeManagerButton.Visible = false;
        themeManagerButton.MouseFilter = Control.MouseFilterEnum.Pass;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(NMainMenu.DisableBackstop))]
    static void PatchNMainMenuDisableBackstop()
    {
        //themeManagerButton.ShowButton();
        themeManagerButton.Visible = true;
        themeManagerButton.MouseFilter = Control.MouseFilterEnum.Stop;
    }

    [HarmonyPostfix]
    [HarmonyPatch("_Ready")]
    static void PatchNMainMenuReady(NMainMenu __instance)
    {
        nMainMenu = __instance;
        nMainMenuBg = __instance.GetNode<NMainMenuBg>("%MainMenuBg");

        if (!(__instance.GetNodeOrNull<NThemeManagerButton>("%ThemeManagerButton") == null))
            return;
        var button = ResourceLoader.Load<PackedScene>("res://STS2 Theme/Scenes/theme_manager_button.tscn").Instantiate<NThemeManagerButton>();
        button.Name =  "ThemeManagerButton";
        button.UniqueNameInOwner = true;
        themeManagerButton = button;
        __instance.AddChild(themeManagerButton);
    }
}