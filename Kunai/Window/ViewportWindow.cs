using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Numerics;
using OpenTK.Input;
using SharpNeedle.Framework.Ninja.Csd;
using System.Collections.Generic;
using System.Linq;
namespace Kunai.Window
{
    public class ViewportWindow : WindowBase
    {
        public static float ZoomFactor = 1;
        int m_CurrentAspectRatio = 0;
        public override void Update(KunaiProject in_Renderer)
        {
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
                float windowHeight = ImGui.GetWindowWidth() * (in_Renderer.ViewportSize.Y / in_Renderer.ViewportSize.X);

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
                        in_Renderer.ViewportSize = new Vector2(1280, 720);
                    else
                        in_Renderer.ViewportSize = new Vector2(640, 480);
                }
                var vwSize = new Vector2(ImGui.GetWindowWidth(), windowHeight) * ZoomFactor;

                if (in_Renderer.WorkProjectCsd == null)
                    ImGui.Text("Open a XNCP, YNCP, GNCP or SNCP file to edit it.");

                bool open = true;

                if (ImKunai.BeginListBoxCustom("##list", new Vector2(-1, -1)))
                {
                    var cursorpos2 = ImGui.GetCursorScreenPos();
                    var wndSize = ImGui.GetWindowSize();
                    var vwPos = (wndSize - vwSize) * 0.5f;
                    ImGui.SetCursorPos(vwPos);
                    ImGui.Image(
                        new ImTextureID(in_Renderer.GetViewportImageHandle()), vwSize,
                        new Vector2(0, 1), new Vector2(1, 0));

                    DrawQuadList(cursorpos2, windowPos, vwSize, vwPos);
                    ImKunai.EndListBoxCustom();
                }
                ImGui.End();


            }

        }

        private void DrawQuadList(Vector2 in_CursorPos2, Vector2 in_WindowPos, Vector2 in_ViewSize, Vector2 in_ViewPos)
        {
            bool PointInQuad(Vector2 in_P, Vector2 in_P1, Vector2 in_P2, Vector2 in_P3, Vector2 in_P4)
            {
                bool SameSide(Vector2 in_A, Vector2 in_B, Vector2 in_C, Vector2 in_P)
                {
                    Vector2 ab = in_B - in_A;
                    Vector2 ac = in_C - in_A;
                    Vector2 ap = in_P - in_A;

                    float cross1 = ab.X * ac.Y - ab.Y * ac.X;
                    float cross2 = ab.X * ap.Y - ab.Y * ap.X;

                    return Math.Sign(cross1) == Math.Sign(cross2);
                }

                return SameSide(in_P1, in_P2, in_P3, in_P) && SameSide(in_P2, in_P3, in_P4, in_P) &&
                       SameSide(in_P3, in_P4, in_P1, in_P) && SameSide(in_P4, in_P1, in_P2, in_P);
            }
            var cursorpos = ImGui.GetItemRectMin();
            Vector2 screenPos = in_CursorPos2 + in_ViewPos - new Vector2(3, 2);

            List<Cast> possibleSelections = new List<Cast>();
            //ImGui.GetWindowDrawList().AddCircle(screenPos, 10, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)));
            foreach (var quad in Renderer.Renderer.Quads)
            {
                var qTopLeft = quad.TopLeft.Position;
                var qBotRight = quad.BottomRight.Position;
                var qTopRight = quad.TopRight.Position;
                var qBotLeft = quad.BottomLeft.Position;

                Vector2 pTopLeft = screenPos + new Vector2(qTopLeft.X * in_ViewSize.X, qTopLeft.Y * in_ViewSize.Y);
                Vector2 pBotRight = screenPos + new Vector2(qBotRight.X * in_ViewSize.X, qBotRight.Y * in_ViewSize.Y);
                Vector2 pTopRight = screenPos + new Vector2(qTopRight.X * in_ViewSize.X, qTopRight.Y * in_ViewSize.Y);
                Vector2 pBotLeft = screenPos + new Vector2(qBotLeft.X * in_ViewSize.X, qBotLeft.Y * in_ViewSize.Y);

                Vector2 mousePos = ImGui.GetMousePos();
                var cast = quad.OriginalData.OriginCast;
                //Vector2 pcenter = screenPos + new Vector2(quadCenter.X * vwSize.X, quadCenter.Y * vwSize.Y);
                //ImGui.GetWindowDrawList().AddCircle(pcenter, 10, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)));

                //Check if the mouse is inside the quad
                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    if (PointInQuad(mousePos, pTopLeft, pTopRight, pBotRight, pBotLeft))
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
                if (Renderer.Config.ShowQuads)
                    ImGui.GetWindowDrawList().AddQuad(pTopLeft, pTopRight, pBotRight, pBotLeft, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 1)));
                
            }
            if(possibleSelections.Count > 0)
                InspectorWindow.SelectCast(possibleSelections.OrderByDescending(in_X => in_X.Priority).ToList()[0]);
        }
    }
}
