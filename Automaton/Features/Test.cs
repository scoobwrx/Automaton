using Automaton.FeaturesSetup;
using Automaton.FeaturesSetup.Attributes;
using Automaton.Utils;
using ECommons;
using ECommons.DalamudServices;
using ECommons.Interop;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;

namespace Automaton.Features;
[Tweak]
public class Test : Tweak
{
    public override string Name => "Test Name";
    public override string Description => "Test Description";

    public override void Enable()
    {
        Svc.Framework.Update += OnUpdate;
    }

    public override void Disable()
    {
        Svc.Framework.Update -= OnUpdate;
    }

    bool IsLButtonPressed = false;
    private unsafe void OnUpdate(IFramework framework)
    {
        if (!Player.Available || Player.Occupied) return;
        if (!Framework.Instance()->WindowInactive && GenericHelpers.IsKeyPressed([LimitedKeys.LeftControlKey, LimitedKeys.RightControlKey]))
        {
            var pos = ImGui.GetMousePos();
            if (Svc.GameGui.ScreenToWorld(pos, out var res))
            {
                if (GenericHelpers.IsKeyPressed(LimitedKeys.LeftMouseButton))
                {
                    if (!IsLButtonPressed)
                    {
                        Player.GameObject->SetPosition(res.X, res.Y, res.Z);
                    }
                    IsLButtonPressed = true;
                }
                else
                {
                    IsLButtonPressed = false;
                }
            }
        }
    }
}
