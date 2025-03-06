using HekonrayBase;
using Hexa.NET.ImGui;
using Hexa.NET.Utilities.Text;
using IconFonts;
using Kunai.ShurikenRenderer;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kunai.Window
{
    public struct STextureSelectorResult
    {
        public int TextureIndex;
        public int SpriteIndex;
        public STextureSelectorResult(int in_TextureIndex, int in_SpriteIndex)
        {
            TextureIndex = in_TextureIndex;
            SpriteIndex = in_SpriteIndex;
        }
        public bool IsCropSelected()
        {
            return TextureIndex != -2 && SpriteIndex != -2;
        }
        public int GetSpriteIndex()
        {
            return SpriteHelper.Textures[TextureIndex].Sprites[SpriteIndex] - 1;
        }
    }
    public static class ImKunai
    {
        public struct SIconData
        {
            public string Icon;
            public Vector4 Color = Vector4.One;
            public SIconData(string in_Icon)
            {
                Icon = in_Icon;
            }
            public SIconData(string in_Icon, Vector4 in_Color)
            {
                Icon = in_Icon;
                Color = in_Color;
            }
            public bool IsNull() => string.IsNullOrEmpty(Icon);
        }
        public static void TextFontAwesome(string in_Text)
        {
            ImGui.PushFont(ImGuiController.FontAwesomeFont);
            ImGui.Text(in_Text);
            ImGui.PopFont();
        }
        public static STextureSelectorResult TextureSelector(KunaiProject in_Renderer, bool in_EditMode)
        {
            int selectedIndex = -2;
            int selectedSpriteIndex = -2;
            int idx = 0;
            foreach (var t in in_Renderer.WorkProjectCsd.Textures)
            {
                if (ImGui.TreeNode(t.Name))
                {
                    if(in_EditMode)
                    {
                        if (ImGui.Button("+"))
                        {
                            SpriteHelper.Textures[idx].Sprites.Add(SpriteHelper.CreateSprite(SpriteHelper.Textures[idx]));
                        }
                    }
                    int idx2 = 0;
                    foreach (var s in SpriteHelper.Textures[idx].Sprites)
                    {
                        Shuriken.Rendering.Sprite spr = SpriteHelper.Sprites[s];

                        if (spr != null)
                        {
                            if (spr.Texture.GlTex == null)
                            {
                                ImKunai.EmptyTextureButton(idx2);
                            }
                            else
                            {
                                if (ImKunai.SpriteImageButton($"##crop{idx2}", spr))
                                {
                                    selectedIndex = idx;
                                    selectedSpriteIndex = idx2;
                                }
                            }
                        }
                        else
                        {
                            ImKunai.EmptyTextureButton(idx2);
                        }
                        ImGui.SameLine();
                        ImGui.Text($"Crop ({idx2})");
                        idx2++;
                    }

                    ImGui.TreePop();
                }
                idx++;
            }
            return new STextureSelectorResult(selectedIndex, selectedSpriteIndex);
        }
        public static unsafe bool SpriteImageButton(string in_Id, Shuriken.Rendering.Sprite in_Spr)
        {
            //This is so stupid, this is how youre supposed to do it according to the HexaNET issues
            unsafe
            {

                var name = Marshal.StringToHGlobalAnsi($"##{in_Id}");
                var uvCoords = in_Spr.GetImGuiUv();
                //Draw sprite
                return ImGui.ImageButton((byte*)name, new ImTextureID(in_Spr.Texture.GlTex.Id), new System.Numerics.Vector2(50, 50), uvCoords[0], uvCoords[1]);
            }
        }
        public static bool EmptyTextureButton(int in_Id)
        {
            bool val = ImGui.Button($"##pattern{in_Id}", new System.Numerics.Vector2(55, 55));
            ImGui.SameLine();
            return val;
        }
        public static unsafe void SpriteImage(string in_Id, Shuriken.Rendering.Sprite in_Spr)
        {
            unsafe
            {
                const int bufferSize = 256;
                byte* buffer = stackalloc byte[bufferSize];
                StrBuilder sb = new(buffer, bufferSize);
                sb.Append($"##{in_Id}");
                sb.End();
                var uvCoords = in_Spr.GetImGuiUv();
                //Draw sprite
                ImGui.Image(new ImTextureID(in_Spr.Texture.GlTex.Id), new System.Numerics.Vector2(50, 50), uvCoords[0], uvCoords[1]);

            }
        }
        public static bool VisibilityNode(string in_Name, ref bool in_Visibile, ref bool in_IsSelected, Action in_RightClickAction = null, bool in_ShowArrow = true, SIconData in_Icon = new(), string in_Id = "")
        {
            bool returnVal = true;
            bool idPresent = !string.IsNullOrEmpty(in_Id);
            string idName = idPresent ? in_Id : in_Name;
            //Make header fit the content
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(0, 3));
            var isLeaf = !in_ShowArrow ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None;
            returnVal = ImGui.TreeNodeEx($"##{idName}header", isLeaf | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowOverlap);
            ImGui.PopStyleVar();
            //Rightclick action
            if (in_RightClickAction != null)
            {
                if (ImGui.BeginPopupContextItem())
                {
                    in_RightClickAction.Invoke();
                    ImGui.EndPopup();
                }
            }
            //Visibility checkbox
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            ImGui.Checkbox($"##{idName}togg", ref in_Visibile);
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            //Show text with icon (cant have them merged because of stupid imgui c# bindings)

            Vector2 p = ImGui.GetCursorScreenPos();
            ImGui.SetNextItemAllowOverlap();

            //Setup button so that the borders and background arent seen unless its hovered
            ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            ImGui.PushStyleColor(ImGuiCol.Border, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            bool iconPresent = !in_Icon.IsNull();
            in_IsSelected = ImGui.Button($"##invButton{idName}", new Vector2(-1, 25));
            ImGui.PopStyleColor(3);

            //Begin drawing text & icon if it exists
            ImGui.SetNextItemAllowOverlap();
            ImGui.PushID($"##text{idName}");
            ImGui.BeginGroup();

            if (iconPresent)
            {
                //Draw icon
                ImGui.PushFont(ImGuiController.FontAwesomeFont);
                ImGui.SameLine(0, 0);
                ImGui.SetNextItemAllowOverlap();
                ImGui.SetCursorScreenPos(p);
                ImGui.TextColored(in_Icon.Color, in_Icon.Icon);
                ImGui.PopFont();
                ImGui.SameLine(0, 0);
            }
            else
            {
                //Set size for the text as if there was an icon
                ImGui.SetCursorScreenPos(p + new Vector2(0, 2));
            }
            ImGui.SetNextItemAllowOverlap();
            ImGui.Text(iconPresent ? $" {in_Name}" : in_Name);

            ImGui.EndGroup();
            ImGui.PopID();
            return returnVal;
        }
        public static void ItemRowsBackground(Vector4 in_Color, float in_LineHeight = -1.0f)
        {
            var drawList = ImGui.GetWindowDrawList();
            var style = ImGui.GetStyle();

            if (in_LineHeight < 0)
            {
                in_LineHeight = ImGui.GetTextLineHeight();
            }
            in_LineHeight += style.ItemSpacing.Y;

            float scrollOffsetH = ImGui.GetScrollX();
            float scrollOffsetV = ImGui.GetScrollY();
            float scrolledOutLines = (float)Math.Floor(scrollOffsetV / in_LineHeight);
            scrollOffsetV -= in_LineHeight * scrolledOutLines;

            Vector2 clipRectMin = new Vector2(ImGui.GetWindowPos().X, ImGui.GetWindowPos().Y);
            Vector2 clipRectMax = new Vector2(clipRectMin.X + ImGui.GetWindowWidth(), clipRectMin.Y + ImGui.GetWindowHeight());

            if (ImGui.GetScrollMaxX() > 0)
            {
                clipRectMax.Y -= style.ScrollbarSize;
            }

            drawList.PushClipRect(clipRectMin, clipRectMax);

            bool isOdd = ((int)(scrolledOutLines) % 2) == 0;

            float yMin = clipRectMin.Y - scrollOffsetV + ImGui.GetCursorPosY();
            float yMax = clipRectMax.Y - scrollOffsetV + in_LineHeight;
            float xMin = clipRectMin.X + scrollOffsetH + ImGui.GetContentRegionAvail().X;
            float xMax = clipRectMin.X + scrollOffsetH + ImGui.GetContentRegionAvail().X;

            for (float y = yMin; y < yMax; y += in_LineHeight, isOdd = !isOdd)
            {
                if (isOdd)
                {
                    drawList.AddRectFilled(new Vector2(xMin, y - style.ItemSpacing.Y), new Vector2(xMax, y + in_LineHeight), ImGui.ColorConvertFloat4ToU32(in_Color));
                }
            }
        }
        /// <summary>
        /// Fake list box that allows horizontal scrolling
        /// </summary>
        /// <param name="in_Label"></param>
        /// <param name="in_Size"></param>
        /// <returns></returns>
        public static bool BeginListBoxCustom(string in_Label, Vector2 in_Size)
        {
            bool returnVal = ImGui.BeginChild(in_Label, in_Size, ImGuiChildFlags.FrameStyle, ImGuiWindowFlags.HorizontalScrollbar);
            unsafe
            {
                //Ass Inc.
                //This is so that the child window has the same color as normal list boxes would
                ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.FrameBg)));
            }
            ImGui.BeginGroup();
            ImGui.PopStyleColor();
            return returnVal;
        }
        public static void EndListBoxCustom()
        {
            ImGui.EndGroup();
            ImGui.EndChild();
        }

        public static void CenteredZoomableImage(string in_Label, Vector2 in_Size, float in_WindowHeight, float in_ZoomFactor, ImTextureID in_Texture)
        {
            if (ImKunai.BeginListBoxCustom(in_Label, new Vector2(-1, -1)))
            {
                var vwSize = new Vector2(ImGui.GetWindowWidth(), in_WindowHeight) * in_ZoomFactor;
                var cursorpos2 = ImGui.GetCursorScreenPos();
                var wndSize = ImGui.GetWindowSize();
                var vwPos = (wndSize - vwSize) * 0.5f;
                ImGui.SetCursorPos(vwPos);
                ImGui.Image(in_Texture
                    , vwSize,
                    new Vector2(0, 1), new Vector2(1, 0));
                ImKunai.EndListBoxCustom();
            }
        }

        internal static bool InvisibleSelectable(string in_Text)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, 0);
            bool returned = ImGui.Selectable(in_Text);
            ImGui.PopStyleColor();
            return returned;
        }
    }
}
