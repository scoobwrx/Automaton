using Automaton.Debugging;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace Automaton.Features.Debugging;

public unsafe class CharacterDataDebug : DebugHelper
{
    public override string Name => $"{nameof(CharacterDataDebug).Replace("Debug", "")} Debugging";

    public override void Draw()
    {
        ImGui.Text($"{Name}");
        ImGui.Separator();

        if (Svc.ClientState.LocalPlayer is not null)
        {
            ImGui.TextUnformatted($"Transformation ID : {Svc.ClientState.LocalPlayer.GetTransformationID()}");
            ImGui.TextUnformatted($"ModelCharaId: {Svc.ClientState.LocalPlayer.Struct()->Character.CharacterData.ModelCharaId}");
            ImGui.TextUnformatted($"ModelSkeletonId: {Svc.ClientState.LocalPlayer.Struct()->Character.CharacterData.ModelSkeletonId}");
            ImGui.TextUnformatted($"ModelCharaId_2: {Svc.ClientState.LocalPlayer.Struct()->Character.CharacterData.ModelCharaId_2}");
            ImGui.TextUnformatted($"ModelSkeletonId_2: {Svc.ClientState.LocalPlayer.Struct()->Character.CharacterData.ModelSkeletonId_2}");
            ImGui.TextUnformatted($"Free aetheryte: {PlayerState.Instance()->FreeAetheryteId}");
            ImGui.TextUnformatted($"Security token: {PlayerState.Instance()->IsPlayerStateFlagSet(PlayerStateFlag.IsLoginSecurityToken)}");
            ImGui.TextUnformatted($"flag 1: {PlayerState.Instance()->PlayerStateFlags1}");
            ImGui.TextUnformatted($"flag 2: {PlayerState.Instance()->PlayerStateFlags2}");
            ImGui.TextUnformatted($"flag 3: {PlayerState.Instance()->PlayerStateFlags3}");
            var companion = Svc.ClientState.LocalPlayer.Struct()->Character.CompanionObject->Character.GameObject.DataID;
            ImGui.TextUnformatted($"Companion: [{companion}] {Svc.Data.GetExcelSheet<Companion>().GetRow(companion).Singular}");
        }

        if (Svc.ClientState.LocalPlayer.StatusList is not null)
        {
            foreach (var status in Svc.ClientState.LocalPlayer.StatusList)
            {
                ImGui.TextUnformatted($"[{status.StatusId}] {status.GameData.Name} param:{status.Param} so:{status.SourceObject} sid:{status.SourceId}");
            }
        }
    }
}
