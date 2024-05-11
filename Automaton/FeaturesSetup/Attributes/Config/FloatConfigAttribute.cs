using Automaton.Utils;
using ImGuiNET;
using System;
using System.Globalization;
using System.Reflection;

namespace Automaton.FeaturesSetup.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class FloatConfigAttribute : BaseConfigAttribute
{
    public float DefaultValue = 0;
    public float Min = 0;
    public float Max = 100;

    public override void Draw(Tweak tweak, object config, FieldInfo fieldInfo)
    {
        var value = (float)fieldInfo.GetValue(config)!;

        ImGui.TextUnformatted(fieldInfo.Name);

        using var indent = ImGuiX.ConfigIndent();

        if (ImGui.DragFloat("##Input", ref value, 0.01f, Min, Max, "%.2f"))
        {
            fieldInfo.SetValue(config, value);
            OnChangeInternal(tweak, fieldInfo);
        }

        if (DrawResetButton(string.Format(CultureInfo.InvariantCulture, "{0:0.00}", DefaultValue)))
        {
            fieldInfo.SetValue(config, DefaultValue);
            OnChangeInternal(tweak, fieldInfo);
        }

        using var descriptionIndent = ImGuiX.ConfigIndent();
        ImGuiHelpers.SafeTextColoredWrapped(Colors.Grey, tweak.Description);
    }
}
