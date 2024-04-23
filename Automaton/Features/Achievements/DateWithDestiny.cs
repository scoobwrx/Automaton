using Automaton.FeaturesSetup;
using Automaton.Helpers;
using Automaton.IPC;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using visland.Helpers;

namespace Automaton.Features.Achievements;
internal class DateWithDestiny : Feature
{
    public override string Name => "Date with Destiny";
    public override string Description => "It's a FATE bot. Requires vnavmesh and whatever you want for combat. Also has a Yo-Kai mode.";
    public override FeatureType FeatureType => FeatureType.Achievements;

    private bool active = false;
    private Throttle action = new();

    private enum Z
    {
        MiddleLaNoscea = 134,
        LowerLaNoscea = 135,
        EasternLaNoscea = 137,
        WesternLaNoscea = 138,
        UpperLaNoscea = 139,
        WesternThanalan = 140,
        CentralThanalan = 141,
        EasternThanalan = 145,
        SouthernThanalan = 146,
        NorthernThanalan = 147,
        CentralShroud = 148,
        EastShroud = 152,
        SouthShroud = 153,
        NorthShroud = 154,
        OuterLaNoscea = 180,
        CoerthasWesternHighlands = 397,
        TheDravanianForelands = 398,
        TheDravanianHinterlands = 399,
        TheChurningMists = 400,
        TheSeaofClouds = 401,
        AzysLla = 402,
        TheFringes = 612,
        TheRubySea = 613,
        Yanxia = 614,
        ThePeaks = 620,
        TheLochs = 621,
        TheAzimSteppe = 622,
    }

    private static readonly List<uint> YokaiMinions = [200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 390, 391, 392, 393];
    private static readonly List<uint> YokaiLegendaryMedals = [15167, 15168, 15169, 15170, 15171, 15172, 15173, 15174, 15175, 15176, 15177, 15178, 15179, 15180, 30803, 30804, 30805, 30806];
    private static readonly List<List<Z>> YokaiZones =
        [
            [Z.CentralShroud, Z.LowerLaNoscea, Z.CentralThanalan],
            [Z.OuterLaNoscea, Z.WesternLaNoscea, Z.EasternLaNoscea],
            [Z.SouthShroud, Z.UpperLaNoscea, Z.WesternThanalan],
            [Z.NorthShroud, Z.OuterLaNoscea, Z.MiddleLaNoscea],
            [Z.WesternThanalan, Z.CentralShroud, Z.LowerLaNoscea],
            [Z.CentralThanalan, Z.EastShroud, Z.WesternLaNoscea],
            [Z.EasternThanalan, Z.SouthShroud, Z.UpperLaNoscea],
            [Z.SouthernThanalan, Z.NorthShroud, Z.UpperLaNoscea],
            [Z.MiddleLaNoscea, Z.WesternThanalan, Z.CentralShroud],
            [Z.LowerLaNoscea, Z.CentralThanalan, Z.EastShroud],
            [Z.WesternLaNoscea, Z.EasternThanalan, Z.SouthShroud],
            [Z.UpperLaNoscea, Z.SouthernThanalan, Z.NorthShroud],
            [Z.SouthShroud, Z.MiddleLaNoscea, Z.WesternThanalan],
            [Z.CoerthasWesternHighlands, Z.TheDravanianForelands, Z.TheDravanianHinterlands, Z.TheChurningMists, Z.TheSeaofClouds, Z.AzysLla],
            [Z.CoerthasWesternHighlands, Z.TheDravanianForelands, Z.TheDravanianHinterlands, Z.TheChurningMists, Z.TheSeaofClouds, Z.AzysLla],
            [Z.TheFringes, Z.TheRubySea, Z.Yanxia, Z.ThePeaks, Z.TheLochs, Z.TheAzimSteppe],
            [Z.TheFringes, Z.TheRubySea, Z.Yanxia, Z.ThePeaks, Z.TheLochs, Z.TheAzimSteppe],
        ];
    private readonly IEnumerable<(uint Minion, uint Medal, List<Z> Zones)> yokai = YokaiMinions.Zip(YokaiLegendaryMedals, (x, y) => (Minion: x, Medal: y)).Zip(YokaiZones, (xy, z) => (xy.Minion, xy.Medal, z));

    private ushort nextFateID;
    private byte fateMaxLevel;
    private ushort fateID;
    private ushort FateID
    {
        get => fateID; set
        {
            if (fateID != value)
            {
                SyncFate(value);
            }
            fateID = value;
        }
    }

    protected override DrawConfigDelegate DrawConfigTree => (ref bool hasChanged) =>
    {
        if (ImGui.Button("Start/Stop"))
            active ^= true;

        ImGui.TextUnformatted($"Active: {active}");
        ImGui.TextUnformatted($"Filtered FATEs:");
        var fates = GetFates();
        if (fates != null)
            foreach (var fate in fates)
                ImGui.TextUnformatted($"{fate.Name} @ {fate.Position} {Vector3.DistanceSquared(fate.Position, Svc.ClientState.LocalPlayer.Position)} {fate.Progress}%% {fate.TimeRemaining}");
        ImGui.TextUnformatted($"Unfiltered FATEs:");
        foreach (var fate in Svc.Fates)
            ImGui.TextUnformatted($"{fate.Name} @ {fate.Position} {fate.Progress} {fate.TimeRemaining}/{fate.Duration}");
    };

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
        if (!active || Svc.Fates.Count == 0 || Svc.Condition[ConditionFlag.Unknown57]) return;
        if (NavmeshIPC.PathIsRunning())
        {
            if (Vector3.DistanceSquared(FateManager.Instance()->GetFateById(nextFateID)->Location, Svc.ClientState.LocalPlayer.Position) > 3)
                return;
            else
                NavmeshIPC.PathStop();
        }

        if (Svc.Condition[ConditionFlag.InCombat] && !Svc.Condition[ConditionFlag.Mounted]) return;
        var cf = FateManager.Instance()->CurrentFate;
        if (cf is not null)
        {
            FateID = cf->FateId;
            fateMaxLevel = cf->MaxLevel;
            if (Svc.Condition[ConditionFlag.Mounted])
                ExecuteDismount();
            if (!Svc.Condition[ConditionFlag.InCombat])
            {
                var target = GetFateMob();
                if (target != null)
                {
                    Svc.Log.Debug("Targeting fate mob");
                    Svc.Targets.Target = target;
                    Svc.Log.Debug("Finding path to fate");
                    NavmeshIPC.PathfindAndMoveTo(target.Position, false);
                }
            }
        }
        else
            FateID = 0;

        if (cf is null)
        {
            if (YokaiMinions.Contains(CurrentCompanion))
            {
                // fate farm until 15 legendary medals
                var medal = yokai.FirstOrDefault(x => x.Minion == CurrentCompanion).Medal;
                if (InventoryManager.Instance()->GetInventoryItemCount(medal) >= 15)
                {
                    // check for other companions, summon them, repeat
                    Svc.Log.Debug("Have 15 of the relevant Legendary Medal. Swapping minions");
                    var minion = yokai.FirstOrDefault(x => CompanionUnlocked(x.Minion) && InventoryManager.Instance()->GetInventoryItemCount(x.Medal) < 15).Minion;
                    if (minion != default)
                    {
                        ECommons.Automation.Chat.Instance.SendMessage($"/minion {Svc.Data.GetExcelSheet<Companion>().GetRow(minion).Singular}");
                        return;
                    }
                }
                // get zone of minion
                var zones = yokai.FirstOrDefault(x => x.Minion == CurrentCompanion).Zones;
                // if not in zone, go to it
                if (!zones.Contains((Z)Svc.ClientState.TerritoryType))
                {
                    Svc.Log.Debug("Have Yokai minion equipped but not in appropiate zone. Teleporting");
                    Telepo.Instance()->Teleport(CoordinatesHelper.GetZoneMainAetheryte((uint)zones.First()), 0);
                    return;
                }
            }
            if (!Svc.Condition[ConditionFlag.Mounted] && !Svc.Condition[ConditionFlag.Casting])
            {
                Svc.Log.Debug("Mounting");
                ExecuteMount();
                return;
            }

            if (Svc.Condition[ConditionFlag.Mounted] && !Svc.Condition[ConditionFlag.InFlight])
            {
                Svc.Log.Debug("Jumping");
                ExecuteJump();
                return;
            }

            var nextFate = GetFates().FirstOrDefault();
            if (nextFate is not null && Svc.Condition[ConditionFlag.InFlight] && !NavmeshIPC.PathfindInProgress())
            {
                Svc.Log.Debug("Finding path to fate");
                nextFateID = nextFate.FateId;
                NavmeshIPC.PathfindAndMoveTo(nextFate.Position, true);
            }
        }
    }

    private unsafe void ExecuteActionSafe(ActionType type, uint id) => action.Exec(() => ActionManager.Instance()->UseAction(type, id));
    private void ExecuteMount() => ExecuteActionSafe(ActionType.GeneralAction, 24); // flying mount roulette
    private void ExecuteDismount() => ExecuteActionSafe(ActionType.GeneralAction, 23);
    private void ExecuteJump() => ExecuteActionSafe(ActionType.GeneralAction, 2);
    private IOrderedEnumerable<Dalamud.Game.ClientState.Fates.Fate> GetFates()
        => Svc.Fates.Where(f => f.GameData.Rule == 1 && f.State != FateState.Preparation && (f.Duration <= 900 || f.Progress > 0) && f.Progress <= 90 && f.TimeRemaining > 120)
        .OrderBy(f => Vector3.DistanceSquared(Svc.ClientState.LocalPlayer.Position, f.Position));
    private unsafe GameObject GetFateMob() => Svc.Objects.OrderByDescending(x => (x as Character)?.MaxHp ?? 0).FirstOrDefault(x => x.Struct() != null && x.Struct()->FateId == FateID);
    private unsafe uint CurrentCompanion => Svc.ClientState.LocalPlayer.Struct()->Character.CompanionObject->Character.GameObject.DataID;
    private unsafe bool CompanionUnlocked(uint id) => UIState.Instance()->IsCompanionUnlocked(id);

    private void SyncFate(ushort value)
    {
        if (value != 0)
        {
            if (Svc.ClientState.LocalPlayer.Level > fateMaxLevel)
                ECommons.Automation.Chat.Instance.SendMessage("/lsync");
        }
    }
}
