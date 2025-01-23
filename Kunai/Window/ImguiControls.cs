using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kunai.Window
{
    public static class ImguiControls
    {
        public static bool CollapsingHeaderVisibility(string name, ref bool visibile, ref bool isSelected, bool showArrow = true)
        {
            bool returnVal = true;
            ImGui.BeginDisabled(!showArrow);
            
            returnVal = ImGui.TreeNodeEx($"##{name}header", !showArrow ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None);
            ImGui.EndDisabled();
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            ImGui.Checkbox($"##{name}togg", ref visibile);
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            isSelected = ImGui.Selectable(name);
            return returnVal; 
        }
    }
}
