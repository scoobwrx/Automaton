//using FFXIVClientStructs.FFXIV.Client.Game;
//using FFXIVClientStructs.FFXIV.Client.UI.Agent;
//using System.Runtime.InteropServices;

//namespace Automaton.Features;

//[Tweak]
//public class ARDesynth : Tweak
//{
//    public override string Name => "Enhanced Duty Start/End";
//    public override string Description => "Automatically execute certain actions when the duty starts or ends.";

//    private unsafe delegate void SalvageItemDelegate(AgentSalvage* thisPtr, InventoryItem* item, int addonId, byte a4);
//    private SalvageItemDelegate _salvageItem = null!;

//    public override void Enable()
//    {
//        _salvageItem = Marshal.GetDelegateForFunctionPointer<SalvageItemDelegate>(Svc.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 46 48 8B 03")); // thanks veyn

//        //.OnRetainerPostprocessStep += CheckRetainerPostProcess;
//        //_autoRetainerApi.OnRetainerReadyToPostprocess += DoRetainerPostProcess;
//        //_autoRetainerApi.OnCharacterPostprocessStep += CheckCharacterPostProcess;
//        //_autoRetainerApi.OnCharacterReadyToPostProcess += DoCharacterPostProcess;

//    }

//    public override void Disable()
//    {
//    }

//    private void DoRetainerPostProcess()
//    {
//        TaskManager.Abort();
//        TaskManager.Enqueue(() => DiscardNextItem(PostProcessType.Retainer, null));
//    }

//    private unsafe void DiscardNextItem(PostProcessType type, ItemFilter? itemFilter)
//    {
//        Svc.Log.Info($"DiscardNextItem (type = {type})");
//        InventoryItem* nextItem = _inventoryUtils.GetNextItemToDiscard(itemFilter);
//        if (nextItem == null)
//        {
//            Svc.Log.Info("No item to discard found");
//            FinishDiscarding(type);
//        }
//        else
//        {
//            var (inventoryType, slot) = (nextItem->Container, nextItem->Slot);

//            Svc.Log.Info($"Discarding itemId {nextItem->ItemID} in slot {nextItem->Slot} of container {nextItem->Container}.");
//            _salvageItem(AgentSalvage.Instance(), nextItem, 0, 0);
//            _cancelDiscardAfter = DateTime.Now.AddSeconds(15);

//            TaskManager.EnqueueDelay(20);
//            TaskManager.Enqueue(() => ConfirmDiscardItem(type, itemFilter, inventoryType, slot));
//        }
//    }
//}
