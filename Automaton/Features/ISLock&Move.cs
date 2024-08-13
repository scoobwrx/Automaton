using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.MJI;

namespace Automaton.Features;

[Tweak]
public unsafe class ISLockAndMove : Tweak
{
    public override string Name => "Island Sanctuary Lock & Move";
    public override string Description => "Primitive auto gatherer for Island Sanctuary. After gathering from an island sanctuary node, try to auto-lock onto the nearest gatherable and walk towards it.";

    public override void Enable()
    {
        Svc.Framework.Update += CheckToJump;
        Svc.Condition.ConditionChange += CheckToLockAndMove;
    }

    public override void Disable()
    {
        Svc.Framework.Update -= CheckToJump;
        Svc.Condition.ConditionChange -= CheckToLockAndMove;
    }

    private unsafe void CheckToJump(IFramework framework)
    {
        if (Svc.Targets.Target == null || Svc.Targets.Target.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.CardStand)
            return;

        if (Player.IsMoving && Player.IsTargetLocked && MJIManager.Instance()->IsPlayerInSanctuary != 0 && ActionManager.Instance()->GetActionStatus(ActionType.GeneralAction, 2) == 0 && Player.Object.IsNear(Player.Target, 8))
        {
            if (!TaskManager.IsBusy)
            {
                TaskManager.EnqueueDelay(new Random().Next(300, 550));
                TaskManager.Enqueue(() => ActionManager.Instance()->UseAction(ActionType.GeneralAction, 2));
            }
        }
    }

    private unsafe void CheckToLockAndMove(ConditionFlag flag, bool value)
    {
        if (flag == ConditionFlag.OccupiedInQuestEvent && !value && Player.OnIsland)
        {
            if (!Player.Available || Player.IsCasting) return;

            TaskManager.EnqueueDelay(300);
            TaskManager.Enqueue(() =>
            {
                var nearbyNodes = Svc.Objects.Where(x => x.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.CardStand && x.IsTargetable).ToList();
                if (nearbyNodes.Count == 0)
                    return;

                var nearestNode = nearbyNodes.OrderBy(x => Vector3.Distance(x.Position, Player.Object.Position)).FirstOrDefault();
                if (nearestNode != null && nearestNode.IsTargetable)
                {
                    Svc.Targets.Target = nearestNode;
                }

                if (MJIManager.Instance()->CurrentMode == 1)
                {
                    TaskManager.Enqueue(() => { Chat.Instance.SendMessage("/lockon on"); });
                    TaskManager.EnqueueDelay(new Random().Next(100, 250));
                    TaskManager.Enqueue(() => { if (Player.IsTargetLocked) { Chat.Instance.SendMessage("/automove on"); } });
                }
            });
        }
    }
}
