using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace Automaton.UI;

internal class DebugWindow : Window
{
    public DebugWindow() : base($"{Name} - Debug {P.GetType().Assembly.GetName().Version}###{Name}{nameof(DebugWindow)}")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public static void Dispose()
    {

    }

    public unsafe override void Draw()
    {
        var agent = AgentRevive.Instance();
        ImGui.TextUnformatted($"{Player.Object.IsDead}");
        if (agent == null) return;
        ImGui.TextUnformatted($"{agent->ReviveState} {agent->ResurrectionTimeLeft} {agent->ResurrectingPlayerId} {agent->ResurrectingPlayerName}");
    }
}
