using Hexa.NET.ImGui;
using Kunai.ShurikenRenderer;

namespace Kunai.Window
{
    public static class ViewportWindow
    {
        public static float zoomFactor = 1;
        public static void Render(ShurikenRenderHelper in_Renderer)
        {
            var size1 = ImGui.GetWindowViewport().Size.X / 4.5f;
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(size1, MenuBarWindow.menuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(size1 * 2.5f, ImGui.GetWindowViewport().Size.Y / 1.5f), ImGuiCond.Always);
            if (ImGui.Begin("Viewport", MainWindow.flags))
            {
                ImGui.Checkbox("Play", ref in_Renderer.playingAnimations);
                ImGui.SameLine();
                ImGui.InputFloat("Time", ref in_Renderer.time);
                zoomFactor += ImGui.GetIO().MouseWheel / 5;
                zoomFactor = Math.Clamp(zoomFactor, 0.5f, 5);
                float windowHeight = ImGui.GetWindowWidth() * (in_Renderer.size.Y / in_Renderer.size.X);
                ImGui.SliderFloat("Zoom", ref zoomFactor, 0.5f, 5);
                var size = new System.Numerics.Vector2(ImGui.GetWindowWidth(), windowHeight) * zoomFactor;

                if(in_Renderer.WorkProjectCsd == null)

                    ImGui.Text("Open a XNCP, YNCP, GNCP or SNCP file to edit it.");
                if (ImGui.BeginListBox("##list", new System.Numerics.Vector2(-1, -1)))
                {
                    ImGui.SetCursorPos((ImGui.GetWindowSize() - size) * 0.5f);
                    ImGui.Image(
                        new ImTextureID(in_Renderer.texColor), size,
                        new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
                    ImGui.EndListBox();
                }
                ImGui.End();
            }
        }
    }
}
