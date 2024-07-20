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
        if (TryGetAddonByName<AtkUnitBase>("SelectYesno", out var addon))
        {
            var am = new AddonMaster.SelectYesno(addon);
            var fish = MemoryHelper.ReadSeStringNullTerminated(new nint(addon->AtkValues[15].String)).ExtractText();
            var collectability = int.Parse(Regex.Match(am.TextLegacy, @"\d+").Value);
            var actual = GetRow<Item>(36473).Singular.RawString;
            ImGui.TextUnformatted($"fsh:{fish} id:{FindRow<Item>(x => !x.Singular.RawString.IsNullOrEmpty() && fish.Contains(x.Singular.RawString, StringComparison.InvariantCultureIgnoreCase)).RowId} clt:{collectability}");
            ImGui.TextUnformatted($"fsh:{fish} afsh:{actual} {fish.Contains(actual, StringComparison.InvariantCultureIgnoreCase)}");
            //foreach (var row in GetSheet<Item>())
            //{
            //    if (fish.Contains(row.Singular.RawString, StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        ImGui.TextUnformatted($"fish {row.RowId} {row.Singular.RawString} matches");
            //    }
            //}
        }
    }
}
