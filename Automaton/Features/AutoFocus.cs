using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Automaton.Features;

[Tweak]
internal class AutoFocus : Tweak
{
    public override string Name => "Auto Focus";
    public override string Description => "Automatically focuses the marketboard search bar";

    public override void Enable() => Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "ItemSearch", Focus);
    public override void Disable() => Svc.AddonLifecycle.UnregisterListener(Focus);
    private unsafe void Focus(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        if (addon == null) return;
        if (addon->CollisionNodeList == null) addon->UpdateCollisionNodeList(false);
        if (addon->CollisionNodeList == null) return;
        addon->SetFocusNode(addon->CollisionNodeList[11]);
    }
}
