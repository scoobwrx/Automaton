using Automaton.Debugging;
using Automaton.Features.Achievements;
using Automaton.IPC;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;

namespace Automaton.Features.Debugging;
internal unsafe class FateDebug : DebugHelper
{
    public override string Name => $"{nameof(FateDebug).Replace("Debug", "")} Debugging";

    FateManager* fm = FateManager.Instance();
    NavmeshIPC navmesh = new();
    Random random = new();

    public override void Draw()
    {
        ImGui.TextUnformatted($"{Name}");
        ImGui.Separator();

        if (fm == null) return;
        var active = FateManager.Instance()->Fates.Span.ToArray()
            .Where(f => f.Value is not null)
            .OrderBy(f => Vector3.Distance(Svc.ClientState.LocalPlayer!.Position, f.Value->Location))
            .Select(f => f.Value->FateId)
            .ToList();

        var ps = PlayerState.Instance();
        if (ps != null)
            ImGui.TextUnformatted($"Level Synced: {ps->IsLevelSynced}");
        if (fm->CurrentFate != null)
            ImGui.TextUnformatted($"Current Fate: [{fm->CurrentFate->FateId}] {fm->CurrentFate->Name} ({fm->CurrentFate->Duration}) {fm->CurrentFate->Progress}%% <{fm->CurrentFate->State}>");

        ImGui.Separator();

        foreach (var fate in active)
        {
            ImGui.TextUnformatted($"[{fate}] {fm->GetFateById(fate)->Name} ({fm->GetFateById(fate)->Duration}) {fm->GetFateById(fate)->Progress}%% <{fm->GetFateById(fate)->State}>");
        }
    }

    private unsafe Vector3 GetRandomPointInFate(ushort fateID)
    {
        var fate = FateManager.Instance()->GetFateById(fateID);
        var angle = random.NextDouble() * 2 * Math.PI;
        var randomPoint = new Vector3((float)(fate->Location.X + (fate->Radius / 2 * Math.Cos(angle))), fate->Location.Y, (float)(fate->Location.Z + (fate->Radius / 2 * Math.Sin(angle))));
        var x = navmesh.NearestPoint(randomPoint, 5, 5);
        return (Vector3)(x != null ? x : Vector3.Zero);
    }
}
