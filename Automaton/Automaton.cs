using Automaton.Configuration;
using Automaton.FeaturesSetup;
using Automaton.FeaturesSetup.Attributes;
using Automaton.UI;
using Dalamud.Plugin;
using ECommons;
using ECommons.Automation.LegacyTaskManager;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.SimpleGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Automaton;

public class Automaton : IDalamudPlugin
{
    public static string Name => "Automaton";
    private const string Command = "/automaton";

    internal static Automaton P = null!;
    Config Config;
    public static Config C => P.Config;
    public YamlFactory YamlFactory;
    internal static string FileName = null!;

    public static readonly HashSet<Tweak> Tweaks = [];
    internal TaskManager TaskManager;
    internal HaselWindow HaselWindow;
    //internal DebugWindow DebugWindow;

    public Automaton(DalamudPluginInterface pluginInterface)
    {
        P = this;
        HaselWindow = new();
        //DebugWindow = new();
        ECommonsMain.Init(pluginInterface, P, ECommons.Module.DalamudReflector, ECommons.Module.ObjectFunctions);
        EzConfigGui.Init(HaselWindow.Draw);
        HaselWindow.SetWindowProperties();
        //EzConfigGui.WindowSystem.AddWindow(DebugWindow);
        TaskManager = new();
        YamlFactory = new();
        FileName = YamlFactory.DefaultConfigFileName;
        EzConfig.DefaultSerializationFactory = YamlFactory;
        Config = EzConfig.Init<Config>();
        EzCmd.Add(Command, OnCommand, $"Opens the {Name} menu");
        Svc.Framework.RunOnFrameworkThread(InitializeTweaks);
    }

    public void Dispose()
    {
        foreach (var tweak in Tweaks)
        {
            try
            {
                Svc.Log.Debug($"Disposing {tweak.InternalName}");
                tweak.DisposeInternal();
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, $"Failed disposing tweak '{tweak.InternalName}'.");
            }
        }
        ECommonsMain.Dispose();
    }

    public static void SaveConfig() => C.SaveConfiguration(FileName, true);

    private void OnCommand(string command, string args)
    {
        //if (args is "debug" or "d" && Config.ShowDebug)
        //{
        //    DebugWindow.IsOpen = !DebugWindow.IsOpen;
        //    return;
        //}
        EzConfigGui.Window.IsOpen = !EzConfigGui.Window.IsOpen;
    }

    private void InitializeTweaks()
    {
        foreach (var tweakType in GetType().Assembly.GetTypes().Where(type => type.Namespace == "Automaton.Features" && type.GetCustomAttribute<TweakAttribute>() != null))
        {
            try
            {
                Svc.Log.Verbose($"Initializing {tweakType.Name}");
                Tweaks.Add((Tweak)Activator.CreateInstance(tweakType)!);
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, $"[{tweakType.Name}] Error during initialization");
            }
        }

        foreach (var tweak in Tweaks)
        {
            if (!Config.EnabledTweaks.Contains(tweak.InternalName))
                continue;

            try
            {
                tweak.EnableInternal();
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, $"Failed enabling tweak '{tweak.InternalName}'.");
            }
        }
    }
}

