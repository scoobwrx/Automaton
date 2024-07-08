using ECommons.EzHookManager;

namespace Automaton.Features;

[Tweak]
internal class MaxGCRank : Tweak
{
    public override string Name => "Expert Bypass";
    public override string Description => "Automatically maxes your GC rank to bypass the expert delivery requirements. Does not bypass rank-restricted item purchases.";

    private delegate byte GetGrandCompanyRankDelegate(nint a1);
    [EzHook("E8 ?? ?? ?? ?? 3A 43 01", false)]
    private readonly EzHook<GetGrandCompanyRankDelegate> GCRankHook = null!;

    public override void Enable()
    {
        EzSignatureHelper.Initialize(this);
        GCRankHook.Enable();
    }

    public override void Disable()
    {
        GCRankHook.Disable();
    }

    public byte GCRankDetour(nint a1) => 17;
}
