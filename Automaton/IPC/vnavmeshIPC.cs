using ECommons.EzIpcManager;
using ECommons.Reflection;
using System;
using System.Numerics;

namespace Automaton.IPC;
internal class NavmeshIPC
{
    public static string Name = "vnavmesh";
    public NavmeshIPC() => EzIPC.Init(this, Name);
    public static bool IsEnabled => DalamudReflector.TryGetDalamudPlugin(Name, out _, false, true);

    [EzIPC("Nav.%m")] public readonly Func<bool> IsReady;
    [EzIPC("Nav.%m")] public readonly Func<float> BuildProgress;
    [EzIPC("Nav.%m")] public readonly Func<bool> Reload;
    [EzIPC("Nav.%m")] public readonly Func<bool> Rebuild;
    [EzIPC("Nav.%m")] public readonly Func<Vector3, Vector3, bool, Vector3> Pathfind;

    [EzIPC("SimpleMove.%m")] public readonly Func<Vector3, bool, bool> PathfindAndMoveTo;
    [EzIPC("SimpleMove.%m")] public readonly Func<bool> PathfindInProgress;

    [EzIPC("Path.%m")] public readonly Action Stop;
    [EzIPC("Path.%m")] public readonly Func<bool> IsRunning;

    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3, float, float, Vector3> NearestPoint;
    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3, float, bool, Vector3> PointOnFloor;
}
