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
        public static bool CollapsingHeaderVisibility(string in_Name, ref bool in_Visibile, ref bool in_IsSelected, bool in_ShowArrow = true)
        {
            bool returnVal = true;
            ImGui.BeginDisabled(!in_ShowArrow);
            
            returnVal = ImGui.TreeNodeEx($"##{in_Name}header", !in_ShowArrow ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None);
            ImGui.EndDisabled();
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            ImGui.Checkbox($"##{in_Name}togg", ref in_Visibile);
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            in_IsSelected = ImGui.Selectable(in_Name);
            return returnVal; 
        }
    }
}
