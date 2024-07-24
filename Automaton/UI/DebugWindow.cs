using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using Dalamud.Memory;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

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

    private int id;
    public unsafe override void Draw()
    {
        if (ImGui.InputInt("home aetheryte", ref id))
            PlayerState.Instance()->HomeAetheryteId = (ushort)id;

        ImGui.TextUnformatted($"{PlayerState.Instance()->HomeAetheryteId}");
    }
}
