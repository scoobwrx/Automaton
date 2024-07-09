using ECommons.EzHookManager;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;

namespace Automaton.Features;

[Tweak(debug: true)]
internal class FlightBypass : Tweak
{
    public override string Name => "Flight Bypass";
    public override string Description => "Bypasses flight restrictions in all zones where it's possible to fly.";

    private delegate nint IsFlightProhibited(nint a1);
    [EzHook("E8 ?? ?? ?? ?? 85 C0 74 07 32 C0 48 83 C4 38", false)]
    private readonly EzHook<IsFlightProhibited> IsFlightProhibitedHook = null!;

    public override void Enable()
    {
        EzSignatureHelper.Initialize(this);
        IsFlightProhibitedHook.Enable();
    }

    public override void Disable()
    {
        IsFlightProhibitedHook.Disable();
    }

    public unsafe nint IsFlightProhibitedDetour(nint a1)
    {
        try
        {
            if (GetRow<TerritoryType>(Player.Territory)?.Unknown32 == 0) // don't detour in zones where flight is impossible normally
                return IsFlightProhibitedHook.Original(a1);
            else if (PlayerState.Instance()->IsAetherCurrentZoneComplete(Svc.ClientState.TerritoryType)) // don't detour in zones where you can already fly
                return IsFlightProhibitedHook.Original(a1);
            else if (!Svc.Condition[ConditionFlag.Mounted]) // don't detour if you aren't mounted
                return IsFlightProhibitedHook.Original(a1);
            else
                return 0;
        }
        catch (Exception e)
        {
            e.Log();
        }
        return IsFlightProhibitedHook.Original(a1);
    }
}
