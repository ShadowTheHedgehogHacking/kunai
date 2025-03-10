using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using System;
using System.Numerics;
using SharpNeedle.Framework.Ninja.Csd;
using System.Collections.Generic;
using System.Linq;
using HekonrayBase.Base;
using HekonrayBase;
using Kunai;
namespace Kunai.Window
{
    public class ViewportWindow : Singleton<ViewportWindow>, IWindow
    {
        public static float ZoomFactor = 1;
        int m_CurrentAspectRatio = 0;

        public void OnReset(IProgramProject in_Renderer)
        {
            throw new NotImplementedException();
        }

        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (KunaiProject)in_Renderer;
            var size1 = ImGui.GetWindowViewport().Size.X / 4.5f;
            var windowPos = new Vector2(size1, MenuBarWindow.MenuBarHeight);
            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(size1 * 2.5f, ImGui.GetWindowViewport().Size.Y / 1.5f - MenuBarWindow.MenuBarHeight), ImGuiCond.Always);
            if (ImGui.Begin("Viewport", MainWindow.WindowFlags))
            {
                bool windowHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows);
                if (windowHovered)
                    ZoomFactor += ImGui.GetIO().MouseWheel / 5;
                ZoomFactor = Math.Clamp(ZoomFactor, 0.5f, 5);

                ImKunai.TextFontAwesome(FontAwesome6.MagnifyingGlass);
                ImGui.SameLine();

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 200);
                ImGui.SliderFloat("##zoom", ref ZoomFactor, 0.5f, 5);
                ImGui.SameLine();
                ImKunai.TextFontAwesome(FontAwesome6.Display);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);
                if (ImGui.Combo("##aspectratio", ref m_CurrentAspectRatio, ["16:9", "4:3"], 2))
                {
                    if (m_CurrentAspectRatio == 0)
                        renderer.ViewportSize = new Vector2(1280, 720);
                    else
                        renderer.ViewportSize = new Vector2(640, 480);
                }
                if (renderer.WorkProjectCsd == null)
                    ImGui.Text("Open a XNCP, YNCP, GNCP or SNCP file to edit it.");

                ImKunai.ImageViewport("##viewportcenter", new Vector2(-1, -1), renderer.ViewportSize.Y / renderer.ViewportSize.X, ZoomFactor, new ImTextureID(renderer.GetViewportImageHandle()), DrawQuadList);
                //var vwSize = new Vector2(ImGui.GetWindowWidth(), windowHeight) * ZoomFactor;
                //
                //if (ImKunai.BeginListBoxCustom("##list", new Vector2(-1, -1)))
                //{
                //    var cursorpos2 = ImGui.GetCursorScreenPos();
                //    var wndSize = ImGui.GetWindowSize();
                //
                //    // Ensure viewport size correctly reflects the zoomed content
                //    var scaledSize = vwSize * ZoomFactor;
                //    var vwPos = (wndSize - scaledSize) * 0.5f;
                //
                //    var fixedVwPos = new Vector2(Math.Max(0, vwPos.X), Math.Max(0, vwPos.Y));
                //
                //    // Set scroll region to match full zoomed element
                //    ImGui.SetCursorPosX(fixedVwPos.X);
                //    ImGui.SetCursorPosY(fixedVwPos.Y);
                //
                //    // Render the zoomed image
                //    ImGui.Image(
                //        , scaledSize,
                //        new Vector2(0, 1), new Vector2(1, 0));
                //
                //    DrawQuadList(cursorpos2, windowPos, scaledSize, fixedVwPos);
                //    ImKunai.EndListBoxCustom();
                //}

                ImGui.End();
            }
        }

        private void DrawQuadList(SCenteredImageData in_Data)
        {
            var renderer = KunaiProject.Instance;
            var cursorpos = ImGui.GetItemRectMin();
            Vector2 screenPos = in_Data.Position + in_Data.ImagePosition - new Vector2(3, 2);

            List<Cast> possibleSelections = new List<Cast>();

            //ImGui.GetWindowDrawList().AddCircle(screenPos, 10, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)));
            foreach (var quad in renderer.Renderer.Quads)
            {                
                var qTopLeft = quad.TopLeft.Position;
                var qBotRight = quad.BottomRight.Position;
                var qTopRight = quad.TopRight.Position;
                var qBotLeft = quad.BottomLeft.Position;
                var viewSize = in_Data.ImageSize;
                Vector2 pTopLeft = screenPos + new Vector2(qTopLeft.X * viewSize.X, qTopLeft.Y * viewSize.Y);
                Vector2 pBotRight = screenPos + new Vector2(qBotRight.X * viewSize.X, qBotRight.Y * viewSize.Y);
                Vector2 pTopRight = screenPos + new Vector2(qTopRight.X * viewSize.X, qTopRight.Y * viewSize.Y);
                Vector2 pBotLeft = screenPos + new Vector2(qBotLeft.X * viewSize.X, qBotLeft.Y * viewSize.Y);

                Vector2 mousePos = ImGui.GetMousePos();
                var cast = quad.OriginalData.OriginCast;
                //Vector2 pcenter = screenPos + new Vector2(quadCenter.X * vwSize.X, quadCenter.Y * vwSize.Y);
                //ImGui.GetWindowDrawList().AddCircle(pcenter, 10, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)));
                
                //Check if the mouse is inside the quad
                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    if (KunaiMath.IsPointInRect(mousePos, pTopLeft, pTopRight, pBotRight, pBotLeft))
                    {
                        //Add selection box
                        ImGui.GetWindowDrawList().AddQuad(pTopLeft, pTopRight, pBotRight, pBotLeft, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0.3f, 0, 0.3f)), 3);

                        //APPARENTLY
                        //These imgui bindings have no way to check if the mouse is clicked
                        //meaning I have to do this dumb stuff
                        if (MainWindow.IsMouseLeftDown)
                        {
                            possibleSelections.Add(quad.OriginalData.OriginCast);
                        }
                        //if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                        //{
                        //    Vector2 mouseInWindow = mousePos - in_WindowPos;
                        //
                        //    Vector2 adjustedMousePos = mouseInWindow - screenPos;
                        //    quad.OriginalData.OriginCast.Position = adjustedMousePos / Renderer.ViewportSize;
                        //}
                    }

                }
                if (renderer.Config.ShowQuads)
                    ImGui.GetWindowDrawList().AddQuad(pTopLeft, pTopRight, pBotRight, pBotLeft, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 1)));
                
            }
            if(possibleSelections.Count > 0)
                InspectorWindow.SelectCast(possibleSelections.OrderByDescending(in_X => in_X.Priority).ToList()[0]);
        }
    }
}
