using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using ECommons;
using ECommons.Reflection;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Automaton.Helpers;

public static class Misc
{
    public static ExcelSheet<Lumina.Excel.GeneratedSheets.Action> Action = null!;
    public static ExcelSheet<AozAction> AozAction = null!;
    public static ExcelSheet<AozActionTransient> AozActionTransient = null!;

    public static uint AozToNormal(uint id) => id == 0 ? 0 : AozAction.GetRow(id)!.Action.Row;

    public static uint NormalToAoz(uint id)
    {
        foreach (var aozAction in AozAction)
        {
            if (aozAction.Action.Row == id) return aozAction.RowId;
        }

        throw new Exception("https://tenor.com/view/8032213");
    }

    public static bool HasPlugin(string name) => DalamudReflector.TryGetDalamudPlugin(name, out var _, false, true);

    public static float IconUnitHeight() => ImGuiHelpers.GetButtonSize(FontAwesomeIcon.Trash.ToIconString()).Y;
    public static float IconUnitWidth() => ImGuiHelpers.GetButtonSize(FontAwesomeIcon.Trash.ToIconString()).X;

    public static bool ApplicationIsActivated()
    {
        var activatedHandle = GetForegroundWindow();
        if (activatedHandle == IntPtr.Zero)
        {
            return false;       // No window is currently activated
        }

        var procId = Process.GetCurrentProcess().Id;
        int activeProcId;
        GetWindowThreadProcessId(activatedHandle, out activeProcId);

        return activeProcId == procId;
    }


    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

    public static unsafe bool IsClickingInGameWorld() =>
        !ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow)
        && !ImGui.GetIO().WantCaptureMouse
        && AtkStage.GetSingleton()->RaptureAtkUnitManager->AtkUnitManager.FocusedUnitsList.Count == 0
        && Framework.Instance()->Cursor->ActiveCursorType == 0;

    // https://github.com/KazWolfe/CollectorsAnxiety/blob/bf48a4b0681e5f70fb67e3b1cb22b4565ecfcc02/CollectorsAnxiety/Util/ImGuiUtil.cs#L10
    public static void DrawProgressBar(int progress, int total, Vector4 colour)
    {
        try
        {
            ImGui.BeginGroup();

            var cursor = ImGui.GetCursorPos();
            var sizeVec = new Vector2(ImGui.GetContentRegionAvail().X - Misc.IconUnitWidth() - (ImGui.GetStyle().WindowPadding.X * 2), Misc.IconUnitHeight());

            var percentage = progress / (float)total;
            var label = string.Format("{0:P} Complete ({1} / {2})", percentage, progress, total);
            var labelSize = ImGui.CalcTextSize(label);

            using var _ = ImRaii.PushColor(ImGuiCol.PlotHistogram, colour);
            ImGui.ProgressBar(percentage, sizeVec, "");

            ImGui.SetCursorPos(new Vector2(cursor.X + sizeVec.X - labelSize.X - 4, cursor.Y));
            ImGuiEx.TextV(label);

            ImGui.EndGroup();
        }
        catch (Exception e) { e.Log(); }
    }
}
