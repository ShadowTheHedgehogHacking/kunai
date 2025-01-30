using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using System;

namespace Kunai.Window
{
    public class ViewportWindow : WindowBase
    {
        public static float ZoomFactor = 1;
        public override void Update(KunaiProject in_Renderer)
        {
            var size1 = ImGui.GetWindowViewport().Size.X / 4.5f;
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(size1, MenuBarWindow.MenuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(size1 * 2.5f, ImGui.GetWindowViewport().Size.Y / 1.5f - MenuBarWindow.MenuBarHeight), ImGuiCond.Always);
            if (ImGui.Begin("Viewport", MainWindow.WindowFlags))
            {                
                if(ImGui.IsWindowHovered())
                ZoomFactor += ImGui.GetIO().MouseWheel / 5;
                ZoomFactor = Math.Clamp(ZoomFactor, 0.5f, 5);
                float windowHeight = ImGui.GetWindowWidth() * (in_Renderer.ViewportSize.Y / in_Renderer.ViewportSize.X);
                
                ImGui.PushFont(ImGuiController.FontAwesomeFont);
                ImGui.Text(FontAwesome6.MagnifyingGlass);
                ImGui.PopFont();
                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);
                ImGui.SliderFloat("##zoom", ref ZoomFactor, 0.5f, 5);
                var size = new System.Numerics.Vector2(ImGui.GetWindowWidth(), windowHeight) * ZoomFactor;

                if(in_Renderer.WorkProjectCsd == null)
                    ImGui.Text("Open a XNCP, YNCP, GNCP or SNCP file to edit it.");

                if (ImGui.BeginListBox("##list", new System.Numerics.Vector2(-1, -1)))
                {
                    ImGui.SetCursorPos((ImGui.GetWindowSize() - size) * 0.5f);
                    ImGui.Image(
                        new ImTextureID(in_Renderer.GetViewportImageHandle()), size,
                        new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
                    ImGui.EndListBox();
                }
                ImGui.End();
            }
        }
    }
}
