using ECommons.DalamudServices;
using System;
using System.Linq;

namespace Automaton.FeaturesSetup.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RequirementAttribute(string InternalName) : Attribute
{
    public string InternalName { get; } = InternalName;

    public bool IsLoaded => Svc.PluginInterface.InstalledPlugins.Any(p => p.InternalName == InternalName && p.IsLoaded);
}
