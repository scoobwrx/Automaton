using ECommons.EzHookManager;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Automaton.Features;

[Tweak]
public unsafe class InstantReturn : Tweak
{
    public override string Name => "Return Bypass";
    public override string Description => "Bypass return cast time and cool down.";

    private delegate byte AgentReturnReceiveEventDelegate(AgentInterface* agent);
    [EzHook("E8 ?? ?? ?? ?? 41 8D 5E 0D", false)]
    private readonly EzHook<AgentReturnReceiveEventDelegate> ReturnHook = null!;

    private delegate nint ExecuteCommandDelegate(int command, int param1, int param2, int param3, int param4);
    [EzHook("E8 ?? ?? ?? ?? 8D 43 0A", false)]
    private readonly EzHook<ExecuteCommandDelegate> ExecuteCommandHook = null!;

    public override void Enable()
    {
        EzSignatureHelper.Initialize(this);
        ReturnHook.Enable();
        ExecuteCommandHook.Enable();
    }

    public override void Disable()
    {
        ReturnHook.Disable();
        ExecuteCommandHook.Disable();
    }

    private byte ReturnDetour(AgentInterface* agent)
    {
        if (ActionManager.Instance()->GetActionStatus(ActionType.GeneralAction, 6) != 0)
            return ReturnHook.Original(agent);

        ExecuteCommand(214);
        return 1;
    }

    private nint ExecuteCommand(int command, int param1 = 0, int param2 = 0, int param3 = 0, int param4 = 0)
    {
        var result = ExecuteCommandHook.Original(command, param1, param2, param3, param4);
        return result;
    }

    private nint ExecuteCommandDetour(int command, int param1, int param2, int param3, int param4)
    {
        Svc.Log.Debug($"[{nameof(ExecuteCommandDetour)}]: cmd:({command}) | p1:{param1} | p2:{param2} | p3:{param3} | p4:{param4}");
        return ExecuteCommandHook.Original(command, param1, param2, param3, param4);
    }
}
