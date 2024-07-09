using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
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
        foreach (var o in Svc.Objects.Where(o => o is IPlayerCharacter).OrderBy(o => o.YalmDistanceX))
        {
            ImGui.TextUnformatted($"{o.Name}: y:{o.YalmDistanceX} d:{Player.Object.Distance(o)} p:{Objects.InParty(o)} m:{o.Character()->Mount.MountId} seat:{GetRow<Mount>(o.Character()->Mount.MountId)!.ExtraSeats}");
        }
        foreach (var icon in Enum.GetValues<SeIconChar>())
        {
            ImGui.TextUnformatted($"{icon.ToIconChar()} - {icon}");
        }
    }
}
