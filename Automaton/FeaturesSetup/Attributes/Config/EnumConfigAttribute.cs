using Automaton.Utils;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System;
using System.Reflection;

namespace Automaton.FeaturesSetup.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class EnumConfigAttribute : BaseConfigAttribute
{
    public bool NoLabel = false;

    public override void Draw(Tweak tweak, object config, FieldInfo fieldInfo)
    {
        var enumType = fieldInfo.FieldType;

        string GetOptionLabel(int value) => $"{Enum.GetName(enumType, value)}";

        if (!NoLabel)
        {
            ImGui.TextUnformatted(fieldInfo.Name);
        }

        using var indent = ImGuiX.ConfigIndent(!NoLabel);

        var selectedValue = (int)(fieldInfo.GetValue(config) ?? 0);
        using var combo = ImRaii.Combo("##Input", GetOptionLabel(selectedValue));
        if (combo.Success)
        {
            foreach (var name in Enum.GetNames(enumType))
            {
                var value = (int)Enum.Parse(enumType, name);

                if (ImGui.Selectable(GetOptionLabel(value), selectedValue == value))
                {
                    fieldInfo.SetValue(config, value);
                    OnChangeInternal(tweak, fieldInfo);
                }

                if (selectedValue == value)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
        }
        combo?.Dispose();

        ImGuiHelpers.SafeTextColoredWrapped(Colors.Grey, tweak.Description);
    }
}
