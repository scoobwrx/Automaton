using Automaton.Utils;
using ImGuiNET;
using System;
using System.Reflection;

namespace Automaton.FeaturesSetup.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class StringConfigAttribute : BaseConfigAttribute
{
    public string DefaultValue = string.Empty;

    public override void Draw(Tweak tweak, object config, FieldInfo fieldInfo)
    {
        var value = (string)fieldInfo.GetValue(config)!;

        ImGui.TextUnformatted(fieldInfo.Name);

        if (ImGui.InputText("##Input", ref value, 50))
        {
            fieldInfo.SetValue(config, value);
            OnChangeInternal(tweak, fieldInfo);
        }

        if (DrawResetButton(DefaultValue))
        {
            fieldInfo.SetValue(config, DefaultValue);
            OnChangeInternal(tweak, fieldInfo);
        }

        ImGuiHelpers.SafeTextColoredWrapped(Colors.Grey, tweak.Description);
    }
}
