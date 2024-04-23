using Automaton.Debugging;
using ECommons;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.STD;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Automaton.Features.Debugging;

public unsafe class ActionDebug : DebugHelper
{
    public override string Name => $"{nameof(ActionDebug).Replace("Debug", "")} Debugging";

    private int _dutyCode;

    public override void Draw()
    {
        ImGui.Text($"{Name}");
        ImGui.Separator();

        var map = FFXIVClientStructs.FFXIV.Client.Game.UI.Map.Instance();
        map->QuestDataSpan
            .ToArray()
            .SelectMany(i => i.MarkerData.ToList())
            .ToList().ForEach(m => ImGui.TextUnformatted($"m:{m.MapId} p:{m.PlaceNameId} pz:{m.PlaceNameZoneId} v:{new Vector3(m.X, m.Y, m.Z)} t:{m.TerritoryTypeId} o:{m.ObjectiveId}"));

        ImGui.DragInt("duty", ref _dutyCode);

        if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("ContentsFinder", out var addon))
        {
            ImGui.TextUnformatted($"{AgentContentsFinder.Instance()->SelectedDutyId} && {((AddonContentsFinder*)addon)->SelectedRow}");
            if (ImGui.Button("swap"))
            {
                SelectDuty((uint)_dutyCode);
            }
        }
    }

    public bool? SelectDuty(uint dutyCode)
    {
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("ContentsFinder", out var addon) || !GenericHelpers.IsAddonReady(addon))
            return false;

        var componentList = addon->GetNodeById(52)->GetAsAtkComponentList();
        if (componentList == null) return false;

        var numDutiesLoaded = *(uint*)((nint)componentList + 508 + 4);
        var agent = AgentContentsFinder.Instance();

        if (agent == null) return false;

        var baseAddress = *(nint*)((nint)agent + 6960);
        if (baseAddress == 0) return false;

        for (int i = 0; i < numDutiesLoaded; i++)
        {
            var dutyId = GetDutyId(baseAddress, i);
            if (dutyCode == dutyId)
            {
                Callback.Fire(addon, true, 3, i + 1);
                return true;
            }
        }
        return false;
    }

    private int GetDutyId(nint baseAddress, int index)
    {
        return *(int*)(baseAddress + 212 + index * 240);
    }
}

public static class StdListExtensions
{
    public static List<T> ToList<T>(this StdList<T> stdList) where T : unmanaged
    {
        var list = new List<T>();

        unsafe
        {
            StdList<T>.Node* currentNode = stdList.Head;
            for (ulong i = 0; i < stdList.Size; i++)
            {
                list.Add(currentNode->Value);
                currentNode = currentNode->Next;
            }
        }

        return list;
    }
}

public static class StdVectorExtensions
{
    public static List<T> ToList<T>(this StdVector<T> stdVector) where T : unmanaged
    {
        var list = new List<T>();
        var size = stdVector.Size();

        unsafe
        {
            T* current = stdVector.First;
            for (ulong i = 0; i < size; i++)
            {
                list.Add(current[i]);
            }
        }

        return list;
    }
}
