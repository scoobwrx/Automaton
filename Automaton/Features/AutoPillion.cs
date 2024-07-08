using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Automaton.Features;
[Tweak(disabled: true)]
public class AutoPillion : Tweak
{
    public override string Name => "Auto Pillion";
    public override string Description => "Automatically hop in to other peoples' mounts when you are near them.";

    private unsafe delegate void RidePillionDelegate(BattleChara* target, int seatIndex);
    [Signature("48 85 C9 0F 84 ?? ?? ?? ?? 48 89 6C 24 ?? 56 48 83 EC")]
    private static readonly RidePillionDelegate? RidePillion = null!;

    public override void Enable()
    {
        base.Enable();
        Svc.Framework.Update += OnUpdate;
    }

    public override void Disable()
    {
        base.Disable();
        Svc.Framework.Update -= OnUpdate;
    }

    private unsafe void OnUpdate(IFramework framework)
    {
        if (!Player.Available || Player.Occupied || Svc.Condition[ConditionFlag.Mounted]) return;
        var target = Svc.Objects.FirstOrDefault(o => Player.Object.IsNear(o), null);
        if (target != null && RidePillion != null)
            RidePillion(target.BattleChara(), 10);
    }
}
