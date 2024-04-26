using Automaton.Features.Commands;
using Automaton.Features.Debugging;
using Automaton.FeaturesSetup;
using Automaton.Helpers;
using Automaton.IPC;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using visland.Helpers;

namespace Automaton.Features.Achievements;
internal class DateWithDestiny : Feature
{
    public override string Name => "Date with Destiny";
    public override string Description => "It's a FATE bot. Requires vnavmesh and whatever you want for combat. Yo-Kai mode can be activated by having a yokai minion summoned.";
    public override FeatureType FeatureType => FeatureType.Achievements;

    private bool active = false;
    private static Vector3 TargetPos;
    private Throttle action = new();
    private NavmeshIPC navmesh;
    private Random random;

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

    private const uint YokaiWatch = 15222;
    private static readonly uint[] YokaiMinions = [200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 390, 391, 392, 393];
    private static readonly uint[] YokaiLegendaryMedals = [15168, 15169, 15170, 15171, 15172, 15173, 15174, 15175, 15176, 15177, 15178, 15179, 15180, 30805, 30804, 30803, 30806];
    private static readonly uint[] YokaiWeapons = [15210, 15216, 15212, 15217, 15213, 15219, 15218, 15220, 15211, 15221, 15214, 15215, 15209, 30809, 30808, 30807, 30810];
    private static readonly Z[][] YokaiZones =
    [
        [Z.CentralShroud, Z.LowerLaNoscea, Z.CentralThanalan], // Jibanyan
        [Z.EastShroud, Z.WesternLaNoscea, Z.EasternThanalan], // Komasan
        [Z.SouthShroud, Z.UpperLaNoscea, Z.SouthernThanalan], // Whisper
        [Z.NorthShroud, Z.OuterLaNoscea, Z.MiddleLaNoscea], // Blizzaria
        [Z.WesternThanalan, Z.CentralShroud, Z.LowerLaNoscea], // Kyubi
        [Z.CentralThanalan, Z.EastShroud, Z.WesternLaNoscea], // Komajiro
        [Z.EasternThanalan, Z.SouthShroud, Z.UpperLaNoscea], // Manjimutt
        [Z.SouthernThanalan, Z.NorthShroud, Z.OuterLaNoscea], // Noko
        [Z.MiddleLaNoscea, Z.WesternThanalan, Z.CentralShroud], // Venoct
        [Z.LowerLaNoscea, Z.CentralThanalan, Z.EastShroud], // Shogunyan
        [Z.WesternLaNoscea, Z.EasternThanalan, Z.SouthShroud], // Hovernyan
        [Z.UpperLaNoscea, Z.SouthernThanalan, Z.NorthShroud], // Robonyan
        [Z.OuterLaNoscea, Z.MiddleLaNoscea, Z.WesternThanalan], // USApyon
        [Z.TheFringes, Z.TheRubySea, Z.Yanxia, Z.ThePeaks, Z.TheLochs, Z.TheAzimSteppe], // Lord Enma
        [Z.CoerthasWesternHighlands, Z.TheDravanianForelands, Z.TheDravanianHinterlands, Z.TheChurningMists, Z.TheSeaofClouds, Z.AzysLla], // Lord Ananta
        [Z.CoerthasWesternHighlands, Z.TheDravanianForelands, Z.TheDravanianHinterlands, Z.TheChurningMists, Z.TheSeaofClouds, Z.AzysLla], // Zazel
        [Z.TheFringes, Z.TheRubySea, Z.Yanxia, Z.ThePeaks, Z.TheLochs, Z.TheAzimSteppe], // Damona
    ];
    private static readonly List<(uint Minion, uint Medal, uint Weapon, Z[] Zones)> Yokai = YokaiMinions
        .Zip(YokaiLegendaryMedals, (x, y) => (Minion: x, Medal: y))
        .Zip(YokaiWeapons, (xy, z) => (xy.Minion, xy.Medal, Weapon: z))
        .Zip(YokaiZones, (wxy, z) => (wxy.Minion, wxy.Medal, wxy.Weapon, z))
        .ToList();

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
        {
            active ^= true;
            navmesh.Stop();
        }
#if DEBUG
        if (Config.showDebugFeatures)
        {
            ImGui.SameLine();
            if (ImGui.Button("Get Current Minion Zones"))
            {
                if (Yokai.Any(x => x.Minion == CurrentCompanion))
                    Svc.Chat.Print(string.Join(", ", Yokai.FirstOrDefault(x => x.Minion == CurrentCompanion).Zones));
                //unsafe { Telepo.Instance()->Teleport(CoordinatesHelper.GetZoneMainAetheryte((uint)z), 0); }
            }
            ImGui.SameLine();
            if (ImGui.Button("print yokai"))
            {
                foreach (var x in Yokai)
                    Svc.Chat.Print($"{Svc.Data.GetExcelSheet<Companion>().GetRow(x.Minion).Singular}: {Svc.Data.GetExcelSheet<Item>().GetRow(x.Medal).Singular}/{Svc.Data.GetExcelSheet<Item>().GetRow(x.Weapon).Singular}/{string.Join(",", x.Zones)}");
            }
        }
#endif
        ImGui.TextUnformatted($"Status: {(active ? "on" : "off")}");
        ImGui.TextUnformatted($"Filtered FATEs:");
        var fates = GetFates();
        if (fates != null)
            foreach (var fate in fates)
                ImGui.TextUnformatted($"{fate.Name} @ {fate.Position} {fate.Progress}% {fate.TimeRemaining}");
        ImGui.TextUnformatted($"Unfiltered FATEs:");
        foreach (var fate in Svc.Fates)
        {
            ImGui.TextUnformatted($"{fate.Name} @ {fate.Position} {fate.Progress}% {fate.TimeRemaining}/{fate.Duration}");
            ImGui.SameLine();
            if (ImGui.Button($"P###{fate.FateId}"))
            {
                if (navmesh.IsRunning())
                    navmesh.Stop();
                else
                    navmesh.PathfindAndMoveTo(GetRandomPointInFate(fate.FateId), Svc.Condition[ConditionFlag.InFlight]);
            }
            ImGui.SameLine();
            if (ImGui.Button($"F###{fate.FateId}"))
                CoordinatesHelper.Place(Svc.ClientState.TerritoryType, fate.Position.X, fate.Position.Z);
        }
    };

    public override void Enable()
    {
        base.Enable();
        navmesh = new();
        random = new();
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
        if (navmesh.IsRunning())
        {
            if (DistanceToTarget() <= 5)
                navmesh.Stop();
            else
                return;
        }

        if (Svc.Condition[ConditionFlag.InCombat] && !Svc.Condition[ConditionFlag.Mounted]) return;
        var cf = FateManager.Instance()->CurrentFate;
        if (cf is not null)
        {
            FateID = cf->FateId;
            fateMaxLevel = cf->MaxLevel;
            if (Svc.Condition[ConditionFlag.Mounted])
                ExecuteDismount();
            if (!Svc.Condition[ConditionFlag.InCombat] && Svc.Targets.Target == null)
            {
                var target = GetFateMob();
                if (target != null)
                {
                    if (Svc.Targets.Target == null)
                    {
                        Svc.Targets.Target = target;
                    }
                    if (!navmesh.PathfindInProgress())
                    {
                        TargetPos = target.Position;
                        navmesh.PathfindAndMoveTo(TargetPos, false);
                        return;
                    }
                }
            }
        }
        else
            FateID = 0;

        if (cf is null)
        {
            if (YokaiMinions.Contains(CurrentCompanion))
            {
                if (HaveYokaiMinionsMissing() && !HasWatchEquipped() && GetItemCount(YokaiWatch) > 0)
                    Equip.EquipItem(15222);

                var medal = Yokai.FirstOrDefault(x => x.Minion == CurrentCompanion).Medal;
                if (GetItemCount(medal) >= 10)
                {
                    Svc.Log.Debug("Have 10 of the relevant Legendary Medal. Swapping minions");
                    var minion = Yokai.FirstOrDefault(x => CompanionUnlocked(x.Minion) && GetItemCount(x.Medal) < 10 && GetItemCount(x.Weapon) < 1).Minion;
                    if (minion != default)
                    {
                        ECommons.Automation.Chat.Instance.SendMessage($"/minion {Svc.Data.GetExcelSheet<Companion>().GetRow(minion).Singular}");
                        return;
                    }
                }

                var zones = Yokai.FirstOrDefault(x => x.Minion == CurrentCompanion).Zones;
                if (!zones.Contains((Z)Svc.ClientState.TerritoryType))
                {
                    Svc.Log.Debug("Have Yokai minion equipped but not in appropiate zone. Teleporting");
                    if (!Svc.Condition[ConditionFlag.Casting])
                        Telepo.Instance()->Teleport(CoordinatesHelper.GetZoneMainAetheryte((uint)zones.First()), 0);
                    return;
                }
            }

            if (!Svc.Condition[ConditionFlag.Mounted] && !Svc.Condition[ConditionFlag.Casting])
            {
                ExecuteMount();
                return;
            }

            if (Svc.Condition[ConditionFlag.Mounted] && !Svc.Condition[ConditionFlag.InFlight])
            {
                ExecuteJump();
                return;
            }

            var nextFate = GetFates().FirstOrDefault();
            if (nextFate is not null && Svc.Condition[ConditionFlag.InFlight] && !navmesh.PathfindInProgress())
            {
                Svc.Log.Debug("Finding path to fate");
                nextFateID = nextFate.FateId;
                TargetPos = nextFate.Position;
                navmesh.PathfindAndMoveTo(TargetPos, true);
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
    private unsafe GameObject GetFateMob()
        => Svc.Objects.OrderBy(x => Vector3.DistanceSquared(x.Position, Svc.ClientState.LocalPlayer.Position))
        .ThenByDescending(x => (x as Character)?.MaxHp ?? 0)
        .ThenByDescending(x => ObjectFunctions.GetAttackableEnemyCountAroundPoint(x.Position, 5))
        .Where(x => x.Struct() != null && x.Struct()->FateId == FateID)
        .Where(x => !x.IsDead && x.IsTargetable && x.IsHostile() && x.ObjectKind == ObjectKind.BattleNpc && x.SubKind == (byte)BattleNpcSubKind.Enemy)
        .Where(x => Math.Sqrt(Math.Pow(x.Position.X - CurrentFate->Location.X, 2) + Math.Pow(x.Position.Z - CurrentFate->Location.Z, 2)) < CurrentFate->Radius)
        .FirstOrDefault();

    private unsafe uint CurrentCompanion => Svc.ClientState.LocalPlayer.Struct()->Character.CompanionObject->Character.GameObject.DataID;
    private unsafe bool CompanionUnlocked(uint id) => UIState.Instance()->IsCompanionUnlocked(id);
    private unsafe bool HasWatchEquipped() => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(10)->ItemID == YokaiWatch;
    private unsafe bool HaveYokaiMinionsMissing() => Yokai.Any(x => CompanionUnlocked(x.Minion));
    private unsafe int GetItemCount(uint itemID) => InventoryManager.Instance()->GetInventoryItemCount(itemID);

    private unsafe FateContext* CurrentFate => FateManager.Instance()->GetFateById(nextFateID);
    private unsafe float DistanceToFate() => Vector3.DistanceSquared(CurrentFate->Location, Svc.ClientState.LocalPlayer.Position);
    private unsafe float DistanceToTarget() => Vector3.DistanceSquared(TargetPos, Svc.ClientState.LocalPlayer.Position);
    private unsafe Vector3 GetRandomPointInFate(ushort fateID)
    {
        var fate = FateManager.Instance()->GetFateById(fateID);
        var angle = random.NextDouble() * 2 * Math.PI;
        var randomPoint = new Vector3((float)(fate->Location.X + (fate->Radius / 2 * Math.Cos(angle))), fate->Location.Y, (float)(fate->Location.Z + (fate->Radius / 2 * Math.Sin(angle))));
        var point = navmesh.NearestPoint(randomPoint, 5, 5);
        return (Vector3)(point != null ? point : fate->Location);
    }

    private void SyncFate(ushort value)
    {
        if (value != 0)
        {
            if (Svc.ClientState.LocalPlayer.Level > fateMaxLevel)
                ECommons.Automation.Chat.Instance.SendMessage("/lsync");
        }
    }
}
