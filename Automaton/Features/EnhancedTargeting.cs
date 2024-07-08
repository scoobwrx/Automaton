using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.GameFunctions;
using ECommons.Interop;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System.Runtime.InteropServices;

namespace Automaton.Features;

public class EnhancedTargetingConfiguration
{
    [StringConfig] public string AutoTargetName = string.Empty;

    [BoolConfig] public bool HighlightPlayersTargeting = false;

    [EnumConfig(DependsOn = nameof(HighlightPlayersTargeting))]
    public ObjectHighlightColor HighlightColor = ObjectHighlightColor.Magenta;

    [BoolConfig(DependsOn = nameof(HighlightPlayersTargeting))]
    public bool HighlightParty = false;

    [BoolConfig(DependsOn = nameof(HighlightPlayersTargeting))]
    public bool HighlightAlliance = false;

    [BoolConfig(DependsOn = nameof(HighlightPlayersTargeting))]
    public bool HighlightInCombat = false;
}

[Tweak(disabled: true)]
public class EnhancedTargeting : Tweak<EnhancedTargetingConfiguration>
{
    public override string Name => "Enhanced Targeting";
    public override string Description => "Customisable targeting. Works in PvP.";

    public override void Enable()
    {
        Svc.Framework.Update += OnUpdate;
    }

    public override void Disable()
    {
        Svc.Framework.Update -= OnUpdate;
    }

    private bool pressed;
    private unsafe void OnUpdate(IFramework framework)
    {
        if (!Player.Available) return;

        foreach (var o in Svc.Objects.Where(o => o is IPlayerCharacter))
        {
            if (o.IsTargetingPlayer() && MeetsCriteria(o))
                o.Struct()->Highlight(Config.HighlightColor);
            else
                o.Struct()->Highlight(ObjectHighlightColor.None);
        }

        if (!Config.AutoTargetName.IsNullOrEmpty())
        {
            var t = Svc.Objects.FirstOrDefault(o => o.IsTargetable && o.Name.TextValue.Equals(Config.AutoTargetName, StringComparison.InvariantCultureIgnoreCase));
            if (t != null)
                Player.Target = t;
        }

        if (IsKeyPressed(LimitedKeys.Tab))
        {
            if (!pressed)
            {
                pressed = true;
                Player.Target = GetEligibleTargets().FirstOrDefault();
            }
            else
                pressed = false;
        }
    }

    private IOrderedEnumerable<DGameObject> GetEligibleTargets()
        => Svc.Objects.Where(o => o.IsTargetable)
        .OrderBy(Player.Object.Distance);

    private bool MeetsCriteria(DGameObject obj)
        => (Config.HighlightParty || !InParty(obj))
        && (Config.HighlightAlliance || !InAlliance(obj))
        && (Config.HighlightInCombat || !InCombat(obj));

    private static byte GetStatus(DGameObject actor) => Marshal.ReadByte(actor.Address + 0x1980);
    private static bool InCombat(DGameObject actor) => (GetStatus(actor) & 2) > 0;
    private static bool InParty(DGameObject actor) => (GetStatus(actor) & 16) > 0;
    private static bool InAlliance(DGameObject actor) => (GetStatus(actor) & 32) > 0;
}
