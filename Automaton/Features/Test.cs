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

    private unsafe void OnUpdate(IFramework framework)
    {
    }
}
