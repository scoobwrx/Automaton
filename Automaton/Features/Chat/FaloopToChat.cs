using Automaton.FeaturesSetup;
namespace Automaton.Features.Chat;

internal class FaloopToChat : Feature
{
    public override string Name => "Echo Faloop";
    public override string Description => "This feature has be deprecated in favour of the original plugin since it is now receiving consistent updates again.\nSee https://xiv.starry.blue/";
    public override FeatureType FeatureType => FeatureType.ChatFeature;

    public override void Enable() => base.Enable();

    public override void Disable() => base.Disable();
}
