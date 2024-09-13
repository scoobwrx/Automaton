using Automaton.IPC;
using Automaton.UI;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility.Raii;
using ECommons.EzHookManager;
using ECommons.GameFunctions;
using ECommons.SimpleGui;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using static ECommons.GameFunctions.ObjectFunctions;

namespace Automaton.Features;

public class DateWithDestinyConfiguration
{
    public HashSet<uint> blacklist = [];
    public HashSet<uint> whitelist = [];
    public List<uint> zones = [];
    [BoolConfig] public bool YokaiMode;
    [BoolConfig] public bool StayInMeleeRange;
    [BoolConfig] public bool PrioritizeForlorns = true;
    [BoolConfig] public bool PrioritizeBonusFates = true;
    [BoolConfig] public bool PrioritizeStartedFates;
    [BoolConfig(DependsOn = nameof(PrioritizeBonusFates))] public bool BonusWhenTwist = false;
    [BoolConfig] public bool EquipWatch = true;
    [BoolConfig] public bool SwapMinions = true;
    [BoolConfig] public bool SwapZones = true;
    [BoolConfig] public bool ExchangeVouchers = false;

    [BoolConfig] public bool FullAuto = true;
    [BoolConfig(DependsOn = nameof(FullAuto))] public bool AutoMount = true;
    [BoolConfig(DependsOn = nameof(FullAuto))] public bool AutoFly = true;
    [BoolConfig(DependsOn = nameof(FullAuto))] public bool PathToFate = true;
    [BoolConfig(DependsOn = nameof(FullAuto))] public bool AutoSync = true;
    [BoolConfig(DependsOn = nameof(FullAuto))] public bool AutoTarget = true;
    [BoolConfig(DependsOn = nameof(FullAuto))] public bool AutoMoveToMobs = true;
    [IntConfig(DefaultValue = 900)] public int MaxDuration = 900;
    [IntConfig(DefaultValue = 120)] public int MinTimeRemaining = 120;
    [IntConfig(DefaultValue = 90)] public int MaxProgress = 90;
    [IntConfig(DefaultValue = 0)] public int VoucherToExchange = 0;
    [IntConfig(DefaultValue = 0)] public int EwVoucherNpcLoc = 0;
    [IntConfig(DefaultValue = 0)] public int DtVoucherNpcLoc = 0;

    [BoolConfig] public bool ShowFateTimeRemaining;
    [BoolConfig] public bool ShowFateBonusIndicator;
}

[Tweak, Requirement(NavmeshIPC.Name, NavmeshIPC.Repo)]
internal class DateWithDestiny : Tweak<DateWithDestinyConfiguration>
{
    public override string Name => "Date with Destiny";
    public override string Description => $"It's a FATE bot. Requires whatever you want for combat. Open the menu with /vfate.";

    public bool active = false;
    private static Vector3 TargetPos;
    private readonly Throttle action = new();
    private Random random = null!;

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

    private bool yokaiMode;
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
    private static readonly List<string> VoucherTypes =
   [
        "Bicolor Gemstone (Endwalker)",
        "Turali Bicolor Gemstone (Dawntrail)"
   ];
    private static readonly List<string> EwVoucherNpc =
   [
        "Old Sharlayan",
        "Radz-at-Han"
   ];
    private static readonly List<string> DtVoucherNpc =
   [
        "Tuliyollal",
        "Solution Nine"
   ];
    private static readonly uint[] ForlornIDs = [7586, 7587];
    private static readonly uint[] TwistOfFateStatusIDs = [1288, 1289];

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

    public override void DrawConfig()
    {
        ImGuiX.DrawSection("Configuration");
        ImGui.Checkbox("Yo-Kai Mode (Very Experimental)", ref yokaiMode);
        ImGui.Checkbox("Prioritize targeting Forlorns", ref Config.PrioritizeForlorns);
        ImGui.Checkbox("Prioritize Fates with EXP bonus", ref Config.PrioritizeBonusFates);
        ImGui.Indent();
        using (var _ = ImRaii.Disabled(!Config.PrioritizeBonusFates))
        {
            ImGui.Checkbox("Only with Twist of Fate", ref Config.BonusWhenTwist);
        }
        ImGui.Unindent();
        ImGui.Checkbox("Prioritize fates that have progress already (up to configured limit)", ref Config.PrioritizeStartedFates);
        ImGui.Checkbox("Always close to melee range of target", ref Config.StayInMeleeRange);
        ImGui.Checkbox("Exchange for gemstone vouchers", ref Config.ExchangeVouchers);
        ImGui.Indent();
        using (var _ = ImRaii.Disabled(!Config.ExchangeVouchers))
        {
            var VoucherPrev = VoucherTypes[Config.VoucherToExchange];
            ImGui.PushID("VoucherCombo");
            if (ImGui.BeginCombo("", VoucherPrev))
            {
                int index = 0;
                foreach (var voucher in VoucherTypes)
                {
                    var selected = ImGui.Selectable(voucher, Config.VoucherToExchange == index);

                    if (selected)
                    {
                        Config.VoucherToExchange = index;
                    }
                    index++;
                }
                ImGui.EndCombo();
            }
            ImGui.PopID();
            ImGui.Unindent();
        }
        if (Config.ExchangeVouchers && Config.VoucherToExchange == 0)
        {
            ImGui.Indent();
            var EwNPCPrev = EwVoucherNpc[Config.EwVoucherNpcLoc];
            ImGui.PushID("EwNpcCombo");
            if (ImGui.BeginCombo("", EwNPCPrev))
            {
                int index = 0;
                foreach (var npc in EwVoucherNpc)
                {
                    var selected = ImGui.Selectable(npc, Config.EwVoucherNpcLoc == index);

                    if (selected)
                    {
                        Config.EwVoucherNpcLoc = index;
                    }
                    index++;
                }
                ImGui.EndCombo();
            }
            ImGui.PopID();
            ImGui.Unindent();
        }
        if (Config.ExchangeVouchers && Config.VoucherToExchange == 1)
        {
            ImGui.Indent();
            var DtNPCPrev = DtVoucherNpc[Config.DtVoucherNpcLoc];
            ImGui.PushID("DtNpcCombo");
            if (ImGui.BeginCombo("", DtNPCPrev))
            {
                int index = 0;
                foreach (var npc in DtVoucherNpc)
                {
                    var selected = ImGui.Selectable(npc, Config.DtVoucherNpcLoc == index);

                    if (selected)
                    {
                        Config.DtVoucherNpcLoc = index;
                    }
                    index++;
                }
                ImGui.EndCombo();
            }
            ImGui.PopID();
            ImGui.Unindent();
        }
        ImGui.Checkbox("Full Auto Mode", ref Config.FullAuto);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"All the below options will be treated as true if this is enabled.");
        ImGui.Indent();
        using (var _ = ImRaii.Disabled(Config.FullAuto))
        {
            ImGui.Checkbox("Auto Mount", ref Config.AutoMount);
            ImGui.Checkbox("Auto Fly", ref Config.AutoFly);
            ImGui.Checkbox("Auto Sync", ref Config.AutoSync);
            ImGui.Checkbox("Auto Target Mobs", ref Config.AutoTarget);
            ImGui.Checkbox("Auto Move To Mobs", ref Config.AutoMoveToMobs);
            ImGui.Checkbox("Path To Next Fate", ref Config.PathToFate);
        }
        ImGui.Unindent();

        ImGuiX.DrawSection("Fate Options");
        ImGui.DragInt("Max Duration (s)", ref Config.MaxDuration);
        ImGui.SameLine();
        ImGuiX.ResetButton(ref Config.MaxDuration, 900);

        ImGui.DragInt("Min Time Remaining (s)", ref Config.MinTimeRemaining);
        ImGui.SameLine();
        ImGuiX.ResetButton(ref Config.MinTimeRemaining, 120);

        ImGui.DragInt("Max Progress (%)", ref Config.MaxProgress, 1, 0, 100);
        ImGui.SameLine();
        ImGuiX.ResetButton(ref Config.MaxProgress, 90);

        ImGuiX.DrawSection("Fate Window Options");
        ImGui.Checkbox("Show Time Remaining", ref Config.ShowFateTimeRemaining);
        ImGui.Checkbox("Show Bonus Indicator", ref Config.ShowFateBonusIndicator);
    }

    public override void Enable()
    {
        EzConfigGui.WindowSystem.AddWindow(new FateTrackerUI(this));
        random = new();
        Svc.Framework.Update += OnUpdate;
    }

    public override void Disable()
    {
        Utils.RemoveWindow<FateTrackerUI>();
        Svc.Framework.Update -= OnUpdate;
    }

    [CommandHandler("/vfate", "Opens the FATE tracker")]
    private void OnCommand(string command, string arguments) => Utils.GetWindow<FateTrackerUI>()!.IsOpen ^= true;

    private unsafe void OnUpdate(IFramework framework)
    {
        if (!active || Svc.Fates.Count == 0 || Svc.Condition[ConditionFlag.Unknown57] || Svc.Condition[ConditionFlag.Casting]) return;

        // Update target position continually so we don't pingpong
        if (Svc.Targets.Target != null)
        {
            var target = Svc.Targets.Target;
            TargetPos = target.Position;
            if ((Config.FullAuto || Config.AutoMoveToMobs) && !IsInMeleeRange(target.HitboxRadius + (Config.StayInMeleeRange ? 0 : 15)))
            {
                TargetAndMoveToEnemy(target);
                return;
            }
        }

        if (P.Navmesh.IsRunning())
        {
            if (DistanceToTarget() < 2 || (Svc.Targets.Target != null && DistanceToHitboxEdge(Svc.Targets.Target.HitboxRadius) <= (Config.StayInMeleeRange ? 0 : 15)))
                P.Navmesh.Stop();
            else
                return;
        }

        var cf = FateManager.Instance()->CurrentFate;
        if (cf is not null)
        {
            fateMaxLevel = cf->MaxLevel;
            FateID = cf->FateId;
            if (Svc.Condition[ConditionFlag.Mounted])
                ExecuteDismount();

            var target = GetFateMob();
            if (target != null) TargetAndMoveToEnemy(target);
        }
        else
            FateID = 0;

        if (cf is null)
        {
            if (Svc.Condition[ConditionFlag.InCombat])
            {
                if (Svc.Condition[ConditionFlag.Mounted]) ExecuteDismount();
                var target = GetMobTargetingPlayer();
                if (target != null) TargetAndMoveToEnemy(target);
            }

            if (Config.YokaiMode)
            {
                if (YokaiMinions.Contains(CurrentCompanion))
                {
                    if (Config.EquipWatch && HaveYokaiMinionsMissing() && !HasWatchEquipped() && GetItemCount(YokaiWatch) > 0)
                        Player.Equip(15222);

                    var medal = Yokai.FirstOrDefault(x => x.Minion == CurrentCompanion).Medal;
                    if (GetItemCount(medal) >= 10)
                    {
                        Svc.Log.Debug("Have 10 of the relevant Legendary Medal. Swapping minions");
                        var minion = Yokai.FirstOrDefault(x => CompanionUnlocked(x.Minion) && GetItemCount(x.Medal) < 10 && GetItemCount(x.Weapon) < 1).Minion;
                        if (Config.SwapMinions && minion != default)
                        {
                            ECommons.Automation.Chat.Instance.SendMessage($"/minion {GetRow<Companion>(minion)?.Singular}");
                            return;
                        }
                    }

                    var zones = Yokai.FirstOrDefault(x => x.Minion == CurrentCompanion).Zones;
                    if (Config.SwapZones && !zones.Contains((Z)Svc.ClientState.TerritoryType))
                    {
                        Svc.Log.Debug("Have Yokai minion equipped but not in appropiate zone. Teleporting");
                        if (!Svc.Condition[ConditionFlag.Casting])
                            Telepo.Instance()->Teleport((uint)Coords.GetPrimaryAetheryte((uint)zones.First())!, 0);
                        return;
                    }
                }
            }

            if ((Config.FullAuto || Config.AutoMount) && !Svc.Condition[ConditionFlag.Mounted] && !Svc.Condition[ConditionFlag.Casting])
            {
                ExecuteMount();
                return;
            }

            if ((Config.FullAuto || Config.AutoFly) && Svc.Condition[ConditionFlag.Mounted] && !Svc.Condition[ConditionFlag.InFlight])
            {
                ExecuteJump();
                return;
            }

            var nextFate = GetFates().FirstOrDefault();
            if ((Config.FullAuto || Config.PathToFate) && nextFate is not null && Svc.Condition[ConditionFlag.InFlight] && !P.Navmesh.PathfindInProgress())
            {
                Svc.Log.Debug("Finding path to fate");
                nextFateID = nextFate.FateId;
                TargetPos = GetRandomPointInFate(nextFateID);
                P.Navmesh.PathfindAndMoveTo(TargetPos, true);
            }
        }
    }

    private void TargetAndMoveToEnemy(IGameObject target)
    {
        if (Svc.Condition[ConditionFlag.Mounted]) ExecuteDismount();
        TargetPos = target.Position;
        if ((Config.FullAuto || Config.AutoTarget) && Svc.Targets.Target?.GameObjectId != target.GameObjectId)
        {
            Svc.Targets.Target = target;
        }
        if ((Config.FullAuto || Config.AutoMoveToMobs) && !P.Navmesh.PathfindInProgress() && !IsInMeleeRange(target.HitboxRadius + (Config.StayInMeleeRange ? 0 : 15)))
        {
            P.Navmesh.PathfindAndMoveTo(TargetPos, false);
        }
    }

    private unsafe void ExecuteActionSafe(ActionType type, uint id) => action.Exec(() => ActionManager.Instance()->UseAction(type, id));
    private void ExecuteMount() => ExecuteActionSafe(ActionType.GeneralAction, 24); // flying mount roulette
    private void ExecuteDismount() => ExecuteActionSafe(ActionType.GeneralAction, 23);
    private void ExecuteJump() => ExecuteActionSafe(ActionType.GeneralAction, 2);

    private IOrderedEnumerable<IFate> GetFates() => Svc.Fates.Where(FateConditions)
        .OrderByDescending(x =>
        Config.PrioritizeBonusFates
        && x.HasExpBonus
        && (
        !Config.BonusWhenTwist
        || Player.Status.FirstOrDefault(x => TwistOfFateStatusIDs.Contains(x.StatusId)) != null)
        )
        .ThenByDescending(x => Config.PrioritizeStartedFates && x.Progress > 0)
        .ThenBy(f => Vector3.Distance(Player.Position, f.Position));
    public bool FateConditions(IFate f) => f.GameData.Rule == 1 && f.State != FateState.Preparation && f.Duration <= Config.MaxDuration && f.Progress <= Config.MaxProgress && f.TimeRemaining > Config.MinTimeRemaining && !Config.blacklist.Contains(f.FateId);

    private unsafe DGameObject? GetMobTargetingPlayer()
        => Svc.Objects
        .FirstOrDefault(x => x is ICharacter { MaxHp: > 0 }
        && !x.IsDead
        && x.IsTargetable
        && x.IsHostile()
        && x.ObjectKind == ObjectKind.BattleNpc
        && x.SubKind == (byte)BattleNpcSubKind.Enemy
        && x.IsTargetingPlayer());

    private unsafe DGameObject? GetFateMob()
        => Svc.Objects
        .Where(x => x is ICharacter { MaxHp: > 0 }
        && !x.IsDead
        && x.IsTargetable
        && x.IsHostile()
        && x.ObjectKind == ObjectKind.BattleNpc
        && x.SubKind == (byte)BattleNpcSubKind.Enemy
        && (x.Struct() != null && x.Struct()->FateId == FateID) && Math.Sqrt(Math.Pow(x.Position.X - CurrentFate->Location.X, 2) + Math.Pow(x.Position.Z - CurrentFate->Location.Z, 2)) < CurrentFate->Radius)
        // Prioritize Forlorns if configured
        .OrderByDescending(x => Config.PrioritizeForlorns && ForlornIDs.Contains(x.DataId))
        // Prioritize enemies targeting us
        .ThenByDescending(x => x.IsTargetingPlayer())
        // Deprioritize mobs in combat with other players (hopefully avoid botlike pingpong behavior in trash fates)
        .ThenBy(x => IsTaggedByOther(x) && !x.IsTargetingPlayer())
        // Prioritize closest enemy        
        .ThenBy(x => Math.Floor(Vector3.Distance(Player.Position, x.Position)))
        // Prioritize lowest HP enemy
        .ThenBy(x => (x as ICharacter)?.CurrentHp)
        .FirstOrDefault();

    private unsafe uint CurrentCompanion => Svc.ClientState.LocalPlayer!.Character()->CompanionObject->Character.GameObject.BaseId;
    private unsafe bool CompanionUnlocked(uint id) => UIState.Instance()->IsCompanionUnlocked(id);
    private unsafe bool HasWatchEquipped() => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(10)->ItemId == YokaiWatch;
    private unsafe bool HaveYokaiMinionsMissing() => Yokai.Any(x => CompanionUnlocked(x.Minion));
    private unsafe int GetItemCount(uint itemID) => InventoryManager.Instance()->GetInventoryItemCount(itemID);

    private unsafe FateContext* CurrentFate => FateManager.Instance()->GetFateById(nextFateID);

    private unsafe float DistanceToFate() => Vector3.Distance(CurrentFate->Location, Svc.ClientState.LocalPlayer!.Position);
    private unsafe float DistanceToTarget() => Vector3.Distance(TargetPos, Svc.ClientState.LocalPlayer!.Position);

    //Will be negative if inside hitbox
    private unsafe float DistanceToHitboxEdge(float hitboxRadius) => DistanceToTarget() - hitboxRadius;
    private unsafe bool IsInMeleeRange(float hitboxRadius)
        => DistanceToHitboxEdge(hitboxRadius) < 2;
    public unsafe Vector3 GetRandomPointInFate(ushort fateID)
    {
        var fate = FateManager.Instance()->GetFateById(fateID);
        var angle = random.NextDouble() * 2 * Math.PI;
        var randomPoint = new Vector3((float)(fate->Location.X + fate->Radius / 2 * Math.Cos(angle)), fate->Location.Y, (float)(fate->Location.Z + fate->Radius / 2 * Math.Sin(angle)));
        var point = P.Navmesh.NearestPoint(randomPoint, 5, 5);
        return (Vector3)(point == null ? fate->Location : point);
    }

    private unsafe void SyncFate(ushort value)
    {
        if (value != 0 && PlayerState.Instance()->IsLevelSynced == 0)
        {
            if (Player.Level > fateMaxLevel)
                ECommons.Automation.Chat.Instance.SendMessage("/lsync");
        }
    }

    private static unsafe bool IsTaggedByOther(IGameObject a)
    {
        GetNameplateColor ??= EzDelegate.Get<GetNameplateColorDelegate>(GetNameplateColorSig);
        var plateType = GetNameplateColor(a.Address);
        return plateType == 10;
    }
}
