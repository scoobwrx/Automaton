using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;

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
        foreach (var o in Svc.Objects.Where(o => o is IPlayerCharacter))
        {
            ImGui.TextUnformatted($"{o.Name}: {o.GameObjectId}//{o.EntityId}//{o.DataId}////{o.TargetObjectId}");
        }
        foreach (var icon in Enum.GetValues<SeIconChar>())
        {
            ImGui.TextUnformatted($"{icon.ToIconChar()} - {icon}");
        }
    }
}
