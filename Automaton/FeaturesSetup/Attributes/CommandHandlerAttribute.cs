using System;

namespace Automaton.FeaturesSetup.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandHandlerAttribute(string Command, string HelpMessage, string? ConfigFieldName = null) : Attribute
{
    public string Command { get; } = Command;
    public string HelpMessage { get; } = HelpMessage;
    public string? ConfigFieldName { get; } = ConfigFieldName;
}
