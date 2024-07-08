using Automaton.Configuration;
using Automaton.UI;
using Dalamud.Plugin;
using ECommons;
using ECommons.Automation.LegacyTaskManager;
using ECommons.Configuration;
using ECommons.SimpleGui;
using KamiToolKit;
using System.Collections.Specialized;
using System.Reflection;

namespace Automaton;

public class Automaton : IDalamudPlugin
{
    public static string Name => "Automaton";
    private const string Command = "/automaton";

    internal static Automaton P = null!;
    private readonly Config Config;
    public static Config C => P.Config;

    public static readonly HashSet<Tweak> Tweaks = [];
    internal TaskManager TaskManager;
    internal NativeController NativeController;
    internal AddonObserver AddonObserver;

    public Automaton(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, P, ECommons.Module.DalamudReflector, ECommons.Module.ObjectFunctions);

        EzConfig.DefaultSerializationFactory = new YamlFactory();
        Config = EzConfig.Init<Config>();

        EzCmd.Add(Command, OnCommand, $"Opens the {Name} menu");
        EzConfigGui.Init(new HaselWindow().Draw);
        HaselWindow.SetWindowProperties();
        EzConfigGui.WindowSystem.AddWindow(new DebugWindow());
        NativeController = new NativeController(Svc.PluginInterface);

        AddonObserver = new();
        TaskManager = new();

        Svc.Framework.RunOnFrameworkThread(InitializeTweaks);
        C.EnabledTweaks.CollectionChanged += OnChange;
    }

    public static void OnChange(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var t in Tweaks)
        {
            if (C.EnabledTweaks.Contains(t.InternalName) && !t.Enabled)
                TryExecute(t.EnableInternal);
            else if (!C.EnabledTweaks.Contains(t.InternalName) && t.Enabled)
                t.DisableInternal();
            EzConfig.Save();
        }
    }

    public void Dispose()
    {
        foreach (var tweak in Tweaks)
        {
            Svc.Log.Debug($"Disposing {tweak.InternalName}");
            TryExecute(tweak.DisposeInternal);
        }
        C.EnabledTweaks.CollectionChanged -= OnChange;
        AddonObserver.Dispose();
        ECommonsMain.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        if (args.StartsWith("d"))
            EzConfigGui.WindowSystem.Windows.First(w => w is DebugWindow).IsOpen ^= true;
        else
            EzConfigGui.Window.IsOpen = !EzConfigGui.Window.IsOpen;
    }

    private void InitializeTweaks()
    {
        foreach (var tweakType in GetType().Assembly.GetTypes().Where(type => type.Namespace == "Automaton.Features" && type.GetCustomAttribute<TweakAttribute>() != null))
        {
            Svc.Log.Verbose($"Initializing {tweakType.Name}");
            TryExecute(() => Tweaks.Add((Tweak)Activator.CreateInstance(tweakType)!));
        }

        foreach (var tweak in Tweaks)
        {
            if (!Config.EnabledTweaks.Contains(tweak.InternalName))
                continue;

            TryExecute(tweak.EnableInternal);
        }
    }
}

