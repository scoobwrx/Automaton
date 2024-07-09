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
            if (GetRow<TerritoryType>(Player.Territory)?.Unknown32 > 0
                && !PlayerState.Instance()->IsAetherCurrentZoneComplete(Svc.ClientState.TerritoryType)
                && Svc.Condition[ConditionFlag.Mounted]) // don't bypass zones where no one can fly (idyllshire, rhalgr's)
                return 0;
        }
        catch (Exception e)
        {
            e.Log();
        }
        return IsFlightProhibitedHook.Original(a1);
    }
}
