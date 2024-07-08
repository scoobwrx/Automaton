using Dalamud.Interface;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace Automaton.Features;

[Tweak(disabled: true)]
public class Tools : Tweak
{
    public override string Name => "Tools";
    public override string Description => "Various useful tools.";

    public override unsafe void DrawConfig()
    {
        if (ImGui.Button("Get recipes you can craft"))
        {
            var _recipes = GetSheet<Recipe>()
                .Where(r => r.ItemResult.Value?.RowId != 0 && r.UnkData5.Any(x => x.AmountIngredient > 0 && x.ItemIngredient != 0))
                .Where(r => r.UnkData5.All(x => InventoryManager.Instance()->GetInventoryItemCount((uint)x.ItemIngredient) >= x.AmountIngredient))
                .ToList();
            foreach (var recipe in _recipes)
            {
                ImGuiX.Icon(recipe.ItemResult.Value!.Icon, 25);
                ImGui.SameLine();
                ImGuiEx.TextV($"[{recipe.RowId}] {recipe.ItemResult.Value!.Name.RawString}");
                ImGui.SameLine();
                if (ImGuiX.IconButton(FontAwesomeIcon.BookOpen, $"{recipe.RowId}"))
                    AgentRecipeNote.Instance()->OpenRecipeByRecipeId(recipe.RowId);
            }
        }

        if (ImGui.Button("Get uncompleted recipes"))
        {
            var _recipes = GetSheet<Recipe>().Where(r => r.ItemResult.Value!.RowId != 0 && r.SecretRecipeBook.Value!.RowId == 0 && r.RecipeNotebookList.Value!.RowId == 0 && !QuestManager.IsRecipeComplete(r.RowId)).ToList();
            foreach (var recipe in _recipes)
            {
                ImGuiX.Icon(recipe.ItemResult.Value!.Icon, 25);
                ImGui.SameLine();
                ImGuiEx.TextV($"[{recipe.RowId}] {recipe.ItemResult.Value!.Name.RawString}");
                ImGui.SameLine();
                if (ImGuiX.IconButton(FontAwesomeIcon.BookOpen, $"{recipe.RowId}"))
                    AgentRecipeNote.Instance()->OpenRecipeByRecipeId(recipe.RowId);
            }
        }
    }
}
