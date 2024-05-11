using System;

namespace Automaton.FeaturesSetup.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TweakAttribute(bool debug = false) : Attribute
{
    public bool Debug = debug;
}
