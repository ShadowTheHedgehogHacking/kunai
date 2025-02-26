using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Numerics;
using OpenTK.Input;
namespace Kunai.Window
{
    public class ViewportWindow : WindowBase
    {
        public static float ZoomFactor = 1;

        
        public override void Update(KunaiProject in_Renderer)
        {
            var size1 = ImGui.GetWindowViewport().Size.X / 4.5f;
            var windowPos = new Vector2(size1, MenuBarWindow.MenuBarHeight);
            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always);
                ImGui.SetNextWindowSize(new Vector2(size1 * 2.5f, ImGui.GetWindowViewport().Size.Y / 1.5f - MenuBarWindow.MenuBarHeight), ImGuiCond.Always);
            if (ImGui.Begin("Viewport", MainWindow.WindowFlags))
            {               
            bool windowHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows); 
                if(windowHovered)
                ZoomFactor += ImGui.GetIO().MouseWheel / 5;
                ZoomFactor = Math.Clamp(ZoomFactor, 0.5f, 5);
                float windowHeight = ImGui.GetWindowWidth() * (in_Renderer.ViewportSize.Y / in_Renderer.ViewportSize.X);
                
                ImGui.PushFont(ImGuiController.FontAwesomeFont);
                ImGui.Text(FontAwesome6.MagnifyingGlass);
                ImGui.PopFont();
                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);
                ImGui.SliderFloat("##zoom", ref ZoomFactor, 0.5f, 5);
                var vwSize = new Vector2(ImGui.GetWindowWidth(), windowHeight) * ZoomFactor;

                if(in_Renderer.WorkProjectCsd == null)
                    ImGui.Text("Open a XNCP, YNCP, GNCP or SNCP file to edit it.");

                bool open = true;

                if (ImGui.BeginListBox("##list", new Vector2(-1, -1)))
                {
                    var cursorpos2 = ImGui.GetCursorScreenPos();
                    var wndSize = ImGui.GetWindowSize();
                    var vwPos = (wndSize - vwSize) * 0.5f;
                    ImGui.SetCursorPos(vwPos);
                    ImGui.Image(
                        new ImTextureID(in_Renderer.GetViewportImageHandle()),vwSize,
                        new Vector2(0, 1), new Vector2(1, 0));
                    
                    var cursorpos = ImGui.GetItemRectMin();
                    Vector2 screenPos = cursorpos2 + vwPos - new Vector2(3, 2);
                    ImGui.GetWindowDrawList().AddCircle(screenPos, 10, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)));
                    foreach (var e in Renderer.Renderer.Quads)
                    {
                        var qTopLeft = e.TopLeft.Position;
                        var qBotRight = e.BottomRight.Position;
                        var qTopRight = e.TopRight.Position;
                        var qBotLeft = e.BottomLeft.Position;

                        Vector2 pTopLeft = screenPos + new Vector2(qTopLeft.X * vwSize.X, qTopLeft.Y * vwSize.Y);
                        Vector2 pBotRight = screenPos + new Vector2(qBotRight.X * vwSize.X, qBotRight.Y * vwSize.Y);
                        Vector2 pTopRight = screenPos + new Vector2(qTopRight.X * vwSize.X, qTopRight.Y * vwSize.Y);
                        Vector2 pBotLeft = screenPos + new Vector2(qBotLeft.X * vwSize.X, qBotLeft.Y * vwSize.Y);

                        Vector2 mousePos = ImGui.GetMousePos();
                        var cast = e.OriginalData.OriginCast;

                        //Vector2 pcenter = screenPos + new Vector2(quadCenter.X * vwSize.X, quadCenter.Y * vwSize.Y);
                        //ImGui.GetWindowDrawList().AddCircle(pcenter, 10, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)));
                        // Check if the mouse is inside the rotated quad
                        if (windowHovered)
                        {
                        if (PointInQuad(mousePos, pTopLeft, pTopRight, pBotRight, pBotLeft))
                        {
                            ImGui.GetWindowDrawList().AddQuadFilled(pTopLeft, pTopRight, pBotRight, pBotLeft, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0.8f, 1, 0.2f)));

                            if (MainWindow.IsMouseLeftDown)
                            {                                
                                InspectorWindow.SelectCast(e.OriginalData.OriginCast);
                            }
                            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                            {
                                // Convert mouse position to the ImGui window coordinates
                                Vector2 mouseInWindow = mousePos - windowPos;  // Mouse position in window's local space

                                // Here, you adjust the mouse position based on the texture viewport position.
                                Vector2 adjustedMousePos = mouseInWindow - screenPos;  // Subtract texture position to get relative coordinates
                                e.OriginalData.OriginCast.Position = adjustedMousePos / Renderer.ViewportSize;
                            }
                            }

                        }
                        if (Renderer.Config.ShowQuads)
                            ImGui.GetWindowDrawList().AddQuad(pTopLeft, pTopRight, pBotRight, pBotLeft, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 1)));
                    }
                    ImGui.EndListBox();
                }
                ImGui.End();


            }
            bool PointInQuad(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
            {
                bool SameSide(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
                {
                    Vector2 ab = b - a;
                    Vector2 ac = c - a;
                    Vector2 ap = p - a;

                    float cross1 = ab.X * ac.Y - ab.Y * ac.X;
                    float cross2 = ab.X * ap.Y - ab.Y * ap.X;

                    return Math.Sign(cross1) == Math.Sign(cross2);
                }

                return SameSide(p1, p2, p3, p) && SameSide(p2, p3, p4, p) &&
                       SameSide(p3, p4, p1, p) && SameSide(p4, p1, p2, p);
            }

        }
    }
}
