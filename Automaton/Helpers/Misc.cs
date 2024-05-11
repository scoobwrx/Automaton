using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Runtime.InteropServices;

namespace Automaton.Helpers;

public static partial class Misc
{
    public static uint AozToNormal(uint id) => id == 0 ? 0 : GetRow<AozAction>(id)!.Action.Row;
    public static uint NormalToAoz(uint id) => FindRow<AozAction>(x => x.Action.Row == id)?.RowId ?? throw new Exception("https://tenor.com/view/8032213");

    public static bool ApplicationIsActivated()
    {
        var activatedHandle = GetForegroundWindow();
        if (activatedHandle == IntPtr.Zero)
        {
            return false;       // No window is currently activated
        }

        var procId = Environment.ProcessId;
        GetWindowThreadProcessId(activatedHandle, out var activeProcId);

        return activeProcId == procId;
    }

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetForegroundWindow();

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int GetWindowThreadProcessId(IntPtr handle, out int processId);

    public static unsafe bool IsClickingInGameWorld()
        => !ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow)
        && !ImGui.GetIO().WantCaptureMouse
        && AtkStage.GetSingleton()->RaptureAtkUnitManager->AtkUnitManager.FocusedUnitsList.Count == 0
        && Framework.Instance()->Cursor->ActiveCursorType == 0;
}
