using Automaton.UI;
using Dalamud.Interface.Components;
using ECommons.EzHookManager;
using ECommons.ImGuiMethods;
using ECommons.SimpleGui;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;

namespace Automaton.Features;

public class AchievementTrackerConfiguration
{
    public List<AchievementTracker.Achv> Achievements = [];
    public Vector4 BarColour = Vector4.One;
    public int UpdateFrequency = 60;
    public bool AutoRemoveCompleted = false;
}

[Tweak]
public unsafe class AchievementTracker : Tweak<AchievementTrackerConfiguration>
{
    public override string Name => "Achievement Tracker";
    public override string Description => $"Adds an achievement tracker.";

    public delegate void ReceiveAchievementProgressDelegate(Achievement* achievement, uint id, uint current, uint max);
    [EzHook("C7 81 ?? ?? ?? ?? ?? ?? ?? ?? 89 91 ?? ?? ?? ?? 44 89 81")]
    public EzHook<ReceiveAchievementProgressDelegate> ReceiveAchievementProgressHook = null!;

    public override void DrawConfig()
    {
        ImGuiX.DrawSection("Configuration");
        var edited = false;
        ImGuiEx.TextV("Bar Colour: ");
        ImGui.SameLine();
        var newColor = ImGuiComponents.ColorPickerWithPalette(1, "##color", Config.BarColour, ImGuiColorEditFlags.NoAlpha);

        edited |= !Config.BarColour.Equals(newColor);

        if (edited)
        {
            Config.BarColour = newColor;
        }

        ImGui.SliderInt("Update Frequency (s)", ref Config.UpdateFrequency, 60, 600, "%d", ImGuiSliderFlags.AlwaysClamp);
        ImGui.Checkbox("Auto Remove Completed", ref Config.AutoRemoveCompleted);
    }

    public class Achv
    {
        public uint ID;
        public required string Name;
        public uint CurrentProgress;
        public uint MaxProgress;
        public string Description = string.Empty;
        public byte Points = 0;
        public bool Completed => CurrentProgress != default && CurrentProgress >= MaxProgress;
    }

    public override void Enable()
    {
        EzSignatureHelper.Initialize(this);
        ReceiveAchievementProgressHook.Enable();
        EzConfigGui.WindowSystem.AddWindow(new AchievementTrackerUI(this));
    }

    public override void Disable()
    {
        ReceiveAchievementProgressHook.Disable();
        Misc.RemoveWindow<AchievementTrackerUI>();
    }

    [CommandHandler("/atracker", "Toggle the Achievement Tracker window")]
    private void OnCommand(string command, string arguments) => Misc.GetWindow<AchievementTrackerUI>()!.IsOpen ^= true;

    public void ReceiveAchievementProgressDetour(Achievement* achievement, uint id, uint current, uint max)
    {
        try
        {
            Svc.Log.Debug($"{nameof(ReceiveAchievementProgressDetour)}: [{id}] {current} / {max}");
            foreach (var achv in Config.Achievements)
            {
                if (achv.ID == id)
                {
                    achv.CurrentProgress = current;
                    achv.MaxProgress = max;
                }
            }
        }
        catch (Exception e)
        {
            Svc.Log.Error("Error receiving achievement progress: {e}", e);
        }

        ReceiveAchievementProgressHook.Original(achievement, id, current, max);
    }

    public void RequestUpdate(uint id = 0)
    {
        if (id == 0)
            Config.Achievements.Where(a => !a.Completed).ToList().ForEach(achv => Achievement.Instance()->RequestAchievementProgress(achv.ID));
        else
            Achievement.Instance()->RequestAchievementProgress(id);
    }
}
