using Automaton.Utils;
using Dalamud.Interface;
using System;

namespace Automaton.FeaturesSetup.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class NetworkWarningAttribute : ConfigInfoAttribute
{
    public NetworkWarningAttribute() : base("HaselTweaks.Config.NetworkRequestWarning")
    {
        Icon = FontAwesomeIcon.Bolt;
        Color = Colors.Yellow;
    }
}
