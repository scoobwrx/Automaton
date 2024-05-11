//using Automaton.Debugging;
//using FFXIVClientStructs.FFXIV.Client.Game;
//using FFXIVClientStructs.FFXIV.Client.UI.Agent;
//using ImGuiNET;

//namespace Automaton.Features.Debugging;

//public unsafe class SatisfactionSupplyDebug : DebugHelper
//{
//    public override string Name => $"{nameof(SatisfactionSupplyDebug).Replace("Debug", "")} Debugging";

//    public override void Draw()
//    {
//        ImGui.Text($"{Name}");
//        ImGui.Separator();

//        var agent = AgentSatisfactionSupply.Instance();
//        if (agent == null) return;

//        ImGui.TextUnformatted($"NPC: [{agent->NpcId}] {agent->NpcInfo.SatisfactionRank}");
//        ImGui.TextUnformatted($"ClassJobId: {agent->ClassJobId}");
//        ImGui.TextUnformatted($"ClassJobLevel: {agent->ClassJobLevel}");
//        ImGui.TextUnformatted($"RemainingAllowances: {agent->RemainingAllowances}");
//        ImGui.TextUnformatted($"LevelUnlocked: {agent->LevelUnlocked}");
//        ImGui.TextUnformatted($"CanGlamour: {agent->CanGlamour}");
//        ImGui.TextUnformatted($"Items: {agent->Item[0]} {agent->Item[1]} {agent->Item[2]}");
//        ImGui.TextUnformatted($"WhiteCrafterScrriptId: {agent->WhiteCrafterScrriptId}");
//        ImGui.TextUnformatted($"PurpleCrafterScriptId: {agent->PurpleCrafterScriptId}");
//        ImGui.TextUnformatted($"WhiteGathererScriptId: {agent->WhiteGathererScriptId}");
//        ImGui.TextUnformatted($"PurpleGathererScriptId: {agent->PurpleGathererScriptId}");
//        ImGui.TextUnformatted($"TimeRemainingHours: {agent->TimeRemainingHours}");
//        ImGui.TextUnformatted($"TimeRemainingMinutes: {agent->TimeRemainingMinutes}");
//        foreach (var item in agent->ItemSpan)
//        {
//            ImGui.TextUnformatted($"[{item.Id}] c{item.Collectability1}/{item.Collectability2}/{item.Collectability3} b{item.Bonus} r{item.Reward1Id}/{item.Reward2Id} f{item.FishingSpotId}/{item.SpearFishingSpotId}");
//        }

//        ImGui.TextUnformatted($"WhiteCrafter: {InventoryManager.Instance()->GetInventoryItemCount(AgentSatisfactionSupply.Instance()->WhiteCrafterScrriptId)}");
//        ImGui.TextUnformatted($"PurpleCrafter: {InventoryManager.Instance()->GetInventoryItemCount(AgentSatisfactionSupply.Instance()->PurpleCrafterScriptId)}");
//        ImGui.TextUnformatted($"WhiteGatherer: {InventoryManager.Instance()->GetInventoryItemCount(AgentSatisfactionSupply.Instance()->WhiteGathererScriptId)}");
//        ImGui.TextUnformatted($"PurpleGatherer: {InventoryManager.Instance()->GetInventoryItemCount(AgentSatisfactionSupply.Instance()->PurpleGathererScriptId)}");
//    }
//}
