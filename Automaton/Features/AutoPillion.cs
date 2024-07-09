using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.GeneratedSheets;
using System.Runtime.InteropServices;

namespace Automaton.Features;

[Tweak]
public class AutoPillion : Tweak
{
    public override string Name => "Auto Pillion";
    public override string Description => "Automatically hop in to other peoples' mounts when you are near them.";

    private unsafe delegate void RidePillionDelegate(BattleChara* target, int seatIndex);
    private static RidePillionDelegate? RidePillion = null!;

    public override void Enable()
    {
        base.Enable();
        RidePillion = Marshal.GetDelegateForFunctionPointer<RidePillionDelegate>(Svc.SigScanner.ScanText("48 85 C9 0F 84 ?? ?? ?? ?? 48 89 6C 24 ?? 56 48 83 EC"));
        Svc.Framework.Update += OnUpdate;
    }

    public override void Disable()
    {
        base.Disable();
        RidePillion = null;
        Svc.Framework.Update -= OnUpdate;
    }

    private unsafe void OnUpdate(IFramework framework)
    {
        if (!Player.Available || Player.Occupied || Svc.Condition[ConditionFlag.Mounted])
        {
            if (TaskManager.Tasks.Count > 0)
                TaskManager.Abort();
            return;
        }

        // TODO: add a check if there are any seats left to get into
        var target = Svc.Party.FirstOrDefault(o => o?.ObjectId != Player.Object.GameObjectId && o?.GameObject?.YalmDistanceX < 3 && GetRow<Mount>(o.GameObject.Character()->Mount.MountId)!.ExtraSeats > 0, null);
        if (target != null && target.GameObject != null && RidePillion != null)
        {
            TaskManager.Enqueue(() => Svc.Log.Debug("Detected mounted party member with extra seats, mounting..."));
            TaskManager.Enqueue(() => RidePillion(target.GameObject.BattleChara(), 10));
            TaskManager.Enqueue(() => Svc.Condition[ConditionFlag.Mounted]);
        }
    }
}
