using ECommons.EzIpcManager;

namespace Automaton.IPC;

#nullable disable
public class AutoRetainerIPC
{
    public AutoRetainerIPC() => EzIPC.Init(this, "AutoRetainer.PluginState");

    [EzIPC] public readonly Func<bool> IsBusy;
    [EzIPC] public readonly Func<int> GetInventoryFreeSlotCount;

    //[EzIPCEvent] public readonly 
}
