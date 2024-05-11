using Automaton.FeaturesSetup;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Automaton.Features.Unconverted;

public class AutoMerge : Feature
{
    public override string Name => "Auto Merge";
    public override string Description => "Merge incomplete stacks upon opening your inventory.";
    public override FeatureType FeatureType => FeatureType.Other;

    public override void Enable()
    {
        base.Enable();
        Svc.Framework.Update += OnSetup;
        Sheet = Svc.Data.GetExcelSheet<Item>().ToDictionary(x => x.RowId, x => x);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.Framework.Update -= OnSetup;
    }

    private Dictionary<uint, Item> Sheet { get; set; }
    public List<InventorySlot> inventorySlots = [];

    public class InventorySlot
    {
        public InventoryType Container { get; set; }
        public ushort Slot { get; set; }
        public uint ItemID { get; set; }
        public bool ItemHQ { get; set; }
    }

    private unsafe void OnSetup(IFramework framework)
    {
        if (GenericHelpers.IsOccupied())
            return;

        inventorySlots.Clear();
        var inv = InventoryManager.Instance();
        for (var container = InventoryType.Inventory1; container <= InventoryType.Inventory4; container++)
        {
            var invContainer = inv->GetInventoryContainer(container);
            for (var i = 1; i <= invContainer->Size; i++)
            {
                var item = invContainer->GetInventorySlot(i - 1);
                if (item->Flags.HasFlag(InventoryItem.ItemFlags.Collectable)) continue;
                if (item->Quantity == Sheet[item->ItemID].StackSize || item->ItemID == 0) continue;

                var slot = new InventorySlot()
                {
                    Container = container,
                    ItemID = item->ItemID,
                    Slot = (ushort)item->Slot,
                    ItemHQ = item->Flags.HasFlag(InventoryItem.ItemFlags.HQ)
                };

                inventorySlots.Add(slot);
            }
        }

        foreach (var item in inventorySlots.GroupBy(x => new { x.ItemID, x.ItemHQ }).Where(x => x.Count() > 1))
        {
            var firstSlot = item.First();
            for (var i = 1; i < item.Count(); i++)
            {
                var slot = item.ToList()[i];
                inv->MoveItemSlot(slot.Container, slot.Slot, firstSlot.Container, firstSlot.Slot, 1);
            }
        }
    }
}
