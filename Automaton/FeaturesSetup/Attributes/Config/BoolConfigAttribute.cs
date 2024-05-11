using Automaton.Utils;
using ImGuiNET;
using System;
using System.Reflection;

namespace Automaton.FeaturesSetup.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class BoolConfigAttribute : BaseConfigAttribute
{
    public override void Draw(Tweak tweak, object config, FieldInfo fieldInfo)
    {
        var value = (bool)fieldInfo.GetValue(config)!;

        if (ImGui.Checkbox($"{fieldInfo.Name}##Input", ref value))
        {
            fieldInfo.SetValue(config, value);
            OnChangeInternal(tweak, fieldInfo);
        }

        DrawConfigInfos(fieldInfo);

        ImGuiX.PushCursorY(-3);
        using var descriptionIndent = ImGuiX.ConfigIndent();
        ImGuiHelpers.SafeTextColoredWrapped(Colors.Grey, tweak.Description);
        ImGuiX.PushCursorY(3);
    }
}
