using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kunai.Window
{
    public static class ImguiControls
    { 
        public static bool CollapsingHeaderVisibility(string in_Name, ref bool in_Visibile, ref bool in_IsSelected,Action rightClickAction = null, bool in_ShowArrow = true, string in_Icon = "")
        {
            bool returnVal = true;

            //Make header fit the content
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(0, 3));
            var isLeaf = !in_ShowArrow ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None;
            returnVal = ImGui.TreeNodeEx($"##{in_Name}header", isLeaf | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowOverlap);
            ImGui.PopStyleVar();
            //Rightclick action
            if (rightClickAction != null)
            if (ImGui.BeginPopupContextItem())
            {
                rightClickAction.Invoke();
                    ImGui.EndPopup();
            }
            //Visibility checkbox
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            ImGui.Checkbox($"##{in_Name}togg", ref in_Visibile);
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            //Show text with icon (cant have them merged because of stupid imgui c# bindings)
            if(!string.IsNullOrEmpty(in_Icon))
            {
                Vector2 p = ImGui.GetCursorScreenPos();
                ImGui.SetNextItemAllowOverlap();              
                    
                ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
                ImGui.PushStyleColor(ImGuiCol.Border, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
                ImGui.PushStyleColor(ImGuiCol.Button, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
                
                in_IsSelected = ImGui.Button($"##invButton{in_Name}", new Vector2(-1, 25));
                ImGui.PopStyleColor(3);
                ImGui.SetNextItemAllowOverlap();
                ImGui.BeginGroup();
                ImGui.PushFont(ImGuiController.FontAwesomeFont);
                ImGui.SameLine(0,0);
                ImGui.SetNextItemAllowOverlap();
                ImGui.SetCursorScreenPos(p);
                ImGui.Text(in_Icon);
                ImGui.PopFont();
                ImGui.SameLine(0,0);
                ImGui.SetNextItemAllowOverlap();
                ImGui.Text($" {in_Name}");
                ImGui.EndGroup();
                return returnVal;
            }
            in_IsSelected = ImGui.Selectable($"{in_Name}");
            return returnVal; 
        }
    }
}
