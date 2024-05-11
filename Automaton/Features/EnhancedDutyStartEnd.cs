using Automaton.FeaturesSetup;
using Automaton.FeaturesSetup.Attributes;
using Automaton.Utils;
using ECommons;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ImGuiNET;
using System.Runtime.InteropServices;

namespace Automaton.Features;

public class EnhancedDutyStartEndConfiguration
{
    public int TimeToWait;
    public string StartMsg = string.Empty;
    public string EndMsg = string.Empty;
    public bool AutoLeave;
}

[Tweak]
public class EnhancedDutyStartEnd : Tweak<EnhancedDutyStartEndConfiguration>
{
    public override string Name => "Enhanced Duty Start/End";
    public override string Description => "Automatically execute certain actions when the duty starts or ends.";

    private delegate void AbandonDuty(bool a1);
    private AbandonDuty _abandonDuty = null!;

    public override void Enable()
    {
        Svc.DutyState.DutyStarted += OnDutyStart;
        Svc.DutyState.DutyCompleted += OnDutyComplete;
        _abandonDuty = Marshal.GetDelegateForFunctionPointer<AbandonDuty>(Svc.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 43 28 B1 01"));
    }

    public override void Disable()
    {
        Svc.DutyState.DutyStarted -= OnDutyStart;
        Svc.DutyState.DutyCompleted -= OnDutyComplete;
    }

    public override void DrawConfig()
    {
        ImGuiX.DrawSection("Duty Start Options");

        ImGui.InputText($"##{nameof(Config.StartMsg)}", ref Config.StartMsg, 50);
        ImGuiHelpers.SafeTextColoredWrapped(Colors.Grey, "Sends a chat message when the duty starts.");

        ImGuiX.DrawSection("Duty End Options");

        ImGui.InputText($"##{nameof(Config.EndMsg)}", ref Config.EndMsg, 50);
        ImGuiHelpers.SafeTextColoredWrapped(Colors.Grey, "Sends a chat message when the duty ends.");

        ImGui.Checkbox("Auto Leave", ref Config.AutoLeave);
        if (Config.AutoLeave)
            ImGui.SliderInt("Leave after (s)", ref Config.TimeToWait, 0, 100);
    }

    private void OnDutyStart(object? sender, ushort e)
    {
        if (!Config.StartMsg.IsNullOrEmpty())
            ECommons.Automation.Chat.Instance.SendMessage($"/p {Config.StartMsg}");
    }

    private void OnDutyComplete(object? sender, ushort e)
    {
        if (!Config.EndMsg.IsNullOrEmpty())
            ECommons.Automation.Chat.Instance.SendMessage($"/p {Config.EndMsg}");

        if (Config.AutoLeave)
        {
            TaskManager.EnqueueDelay(Config.TimeToWait.Ms());
            TaskManager.Enqueue(() => _abandonDuty(false));
        }
    }
}
