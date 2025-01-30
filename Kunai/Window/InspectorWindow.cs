using Kunai.ShurikenRenderer;
using Hexa.NET.ImGui;
using SharpNeedle.Ninja.Csd;
using Shuriken.Rendering;
using Sprite = Shuriken.Rendering.Sprite;
using Hexa.NET.Utilities.Text;
using Vector2 = System.Numerics.Vector2;
using System.Collections.Generic;
using System.Numerics;
using System;

namespace Kunai.Window
{
    public class InspectorWindow : WindowBase
    {
        public enum ESelectionType
        {
            None,
            Scene,
            Node,
            Cast
        }
        public static ESelectionType SelectionType;

        private static List<string> _fontNames = new List<string>();
        public static void SelectCast(Cast in_Cast)
        {
            KunaiProject.Instance.SelectionData.SelectedCast = in_Cast;
            SelectionType = ESelectionType.Cast;
        }
        public static void SelectScene(KeyValuePair<string, Scene> in_Cast)
        {
            KunaiProject.Instance.SelectionData.SelectedScene = in_Cast;
            SelectionType = ESelectionType.Scene;
        }

        private static int Gcd(int in_A, int in_B)
        {
            while (in_B != 0)
            {
                int temp = in_B;
                in_B = in_A % in_B;
                in_A = temp;
            }
            return in_A;
        }

        public void DrawSceneInspector()
        {
            KeyValuePair<string, Scene> selectedScene = KunaiProject.Instance.SelectionData.SelectedScene;
            if (selectedScene.Value == null)
                return;
            ImGui.SeparatorText("Scene");
            string name = selectedScene.Key;
            int vers = selectedScene.Value.Version;
            int priority = (int)selectedScene.Value.Priority;
            float aspectRatio = selectedScene.Value.AspectRatio;
            float fps = selectedScene.Value.FrameRate;

            //Show aspect ratio as (width:height) by using greatest common divisor
            int width = 1280;
            int height = (int)(1280 / aspectRatio);
            double decimalRatio = (double)width / height;
            int gcdValue = Gcd(width, height);
            int simplifiedWidth = width / gcdValue;
            int simplifiedHeight = height / gcdValue;

            ImGui.InputText("Name", ref name, 256);
            ImGui.InputFloat("Framerate", ref fps);
            ImGui.InputFloat("Aspect Ratio", ref aspectRatio);
            ImGui.InputInt("Version", ref vers);
            ImGui.InputInt("Priority", ref priority);
            ImGui.Text($"Ratio: ({simplifiedWidth}:{simplifiedHeight})");
            ImGui.SeparatorText("Info");
            var space = ImGui.GetContentRegionAvail();
            if(ImGui.BeginListBox("##crops", new Vector2(-1, space.Y/2 - 5)))
            {
                int idx = 0;
                foreach(var s in selectedScene.Value.Sprites)
                {
                    if(ImGui.TreeNode($"Crop ({idx})"))
                    {
                        ImGui.Text($"Texture Index: {s.TextureIndex.ToString()}");
                        ImGui.Text($"Top-Left: {s.TopLeft.ToString()}");
                        ImGui.Text($"Bottom-Right: {s.BottomRight.ToString()}");
                        ImGui.TreePop();
                    }
                    idx++;
                }
                ImGui.EndListBox();
            }
            ImGui.Separator();
            if (ImGui.BeginListBox("##textures", new Vector2(-1, space.Y/2 - 5)))
            {
                int idx = 0;
                foreach (var s in selectedScene.Value.Textures)
                {
                    if (ImGui.TreeNode($"Texture ({idx})"))
                    {
                        ImGui.Text($"Size: {s.ToString()}");
                        ImGui.TreePop();
                    }
                    idx++;
                }
                ImGui.EndListBox();
            }
            selectedScene.Value.AspectRatio = aspectRatio;
            selectedScene.Value.AspectRatio = aspectRatio;
            selectedScene.Value.FrameRate = fps;
        }
        public bool EmptyTextureButton(int in_Id)
        {
            bool val = ImGui.Button($"##pattern{in_Id}", new System.Numerics.Vector2(55, 55));
            ImGui.SameLine();
            return val;
        }
        public void DrawCastInspector()
        {
            /// Before you ask
            /// ImGui bindings for C# are mega ass.
            /// Keep that in mind.
            Cast selectedCast = KunaiProject.Instance.SelectionData.SelectedCast;
            if (selectedCast == null)
                return;
            ImGui.SeparatorText("Cast");
            string[] typeStrings = { "Null (No Draw)", "Sprite", "Font" };
            string[] blendingStr = { "NRM", "ADD" };
            string[] filteringStr = { "NONE", "LINEAR" };

            ElementMaterialFlags materialFlags = (ElementMaterialFlags)selectedCast.Field38;
            CastInfo info = selectedCast.Info;
            ElementInheritanceFlags inheritanceFlags = (ElementInheritanceFlags)selectedCast.InheritanceFlags.Value;

            string name = selectedCast.Name;
            int field00 = (int)selectedCast.Field00;
            int type = (int)selectedCast.Field04;
            bool enabled = selectedCast.Enabled;
            bool hideflag = info.HideFlag == 1;
            Vector2 scale = info.Scale;
            bool mirrorX = materialFlags.HasFlag(ElementMaterialFlags.MirrorX);
            bool mirrorY = materialFlags.HasFlag(ElementMaterialFlags.MirrorY);
            Vector2 rectSize = new System.Numerics.Vector2((int)selectedCast.Width, (int)selectedCast.Height);
            Vector2 topLeftVert = selectedCast.TopLeft * KunaiProject.Instance.ViewportSize;
            Vector2 topRightVert = selectedCast.TopRight * KunaiProject.Instance.ViewportSize;
            Vector2 bottomLeftVert = selectedCast.BottomLeft * KunaiProject.Instance.ViewportSize;
            Vector2 bottomRightVert = selectedCast.BottomRight * KunaiProject.Instance.ViewportSize;
            float rotation = info.Rotation;
            Vector2 origin = selectedCast.Origin;
            Vector2 translation = info.Translation;
            Vector4 color = info.Color.ToVec4().Invert();
            Vector4 colorTl = info.GradientTopLeft.ToVec4().Invert();
            Vector4 colorTr = info.GradientTopRight.ToVec4().Invert();
            Vector4 colorBl = info.GradientBottomLeft.ToVec4().Invert();
            Vector4 colorBr = info.GradientBottomRight.ToVec4().Invert();

            bool inheritPosX = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritXPosition);
            bool inheritPosY = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritYPosition);
            bool inheritRot = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritRotation);
            bool inheritCol = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritColor);
            bool inheritScaleX = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleX);
            bool inheritScaleY = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleY);
            int spriteIndex = (int)info.SpriteIndex;
            string text = selectedCast.Text;
            float kerning = BitConverter.ToSingle(BitConverter.GetBytes(selectedCast.Field4C)) * 100;
            string fontname = selectedCast.FontName;

            int indexFont = _fontNames.IndexOf(fontname);

            ImGui.InputText("Room Name", ref name, 100, ImGuiInputTextFlags.AutoSelectAll);
            ImGui.InputInt("Field00", ref field00);
            ImGui.Combo("Type", ref type, typeStrings, 3);
            ImGui.SeparatorText("Status");
            ImGui.Checkbox("Enabled", ref enabled);
            ImGui.SameLine();
            ImGui.Checkbox("Hidden", ref hideflag);

            if (ImGui.CollapsingHeader("Dimensions"))
            {
                ImGui.Text("insert combo here");
                ImGui.SameLine();

                ImGui.BeginGroup();
                ImGui.SeparatorText("Invert UV");
                ImGui.PushID("mirrorH");
                ImGui.Checkbox("H", ref mirrorX);
                ImGui.SetItemTooltip("Mirror the cast horizontally.");
                ImGui.PopID();
                ImGui.SameLine();
                ImGui.Checkbox("V", ref mirrorY);
                ImGui.SetItemTooltip("Mirror the cast vertically.");
                ImGui.EndGroup();

                ImGui.BeginGroup();
                ImGui.SeparatorText("Rect Size");
                ImGui.InputFloat2("Quad Size", ref rectSize);
                ImGui.SetItemTooltip("This does not change any value in the tool, this is a leftover from the CellSprite Editor.");
                ImGui.EndGroup();

                ImGui.SeparatorText("Vertices");
                ImGui.SetItemTooltip("These 4 values determine the 4 points that generate the quad (3D element) that the cast will render on. Use with caution");
                ImGui.DragFloat2("Top Left", ref topLeftVert);
                ImGui.DragFloat2("Top Right", ref topRightVert);
                ImGui.DragFloat2("Bottom Left", ref bottomLeftVert);
                ImGui.DragFloat2("Bottom Right", ref bottomRightVert);

            }
            if (ImGui.CollapsingHeader("Transform"))
            {
                ImGui.DragFloat("Rotation", ref rotation);
                ImGui.SetItemTooltip("Rotation in degrees.");
                ImGui.InputFloat2("Origin", ref origin);
                ImGui.SetItemTooltip("Value used to offset the position of the cast (translation), this cannot be changed by animations.");
                ImGui.InputFloat2("Translation", ref translation);
                ImGui.SetItemTooltip("Position of the cast.");
                ImGui.InputFloat2("Scale", ref scale);
            }
            if (ImGui.CollapsingHeader("Color"))
            {
                ImGui.ColorEdit4("Color", ref color);
                ImGui.ColorEdit4("Top Left", ref colorTl);
                ImGui.ColorEdit4("Top Right", ref colorTr);
                ImGui.ColorEdit4("Bottom Left", ref colorBl);
                ImGui.ColorEdit4("Bottom Right", ref colorBr);
            }
            if (ImGui.CollapsingHeader("Inheritance"))
            {
                ImGui.Checkbox("Inherit Horizontal Position", ref inheritPosX);
                ImGui.Checkbox("Inherit Vertical Position", ref inheritPosY);
                ImGui.Checkbox("Inherit Rotation", ref inheritRot);
                ImGui.Checkbox("Inherit Color", ref inheritCol);
                ImGui.Checkbox("Inherit Width", ref inheritScaleX);
                ImGui.Checkbox("Inherit Height", ref inheritScaleY);
            }

            if (type == 2)
            {
                if (ImGui.CollapsingHeader("Text"))
                {
                    //make combo box eventually
                    if (ImGui.Combo("Font", ref indexFont, _fontNames.ToArray(), _fontNames.Count))
                    {
                        fontname = _fontNames[indexFont];
                    }
                    ImGui.PushID("textInput");
                    ImGui.InputText("Text", ref text, 512);
                    ImGui.PopID();
                    ImGui.DragFloat("Kerning", ref kerning, 0.005f);
                }
            }
            if (type != 0)
            {
                if (ImGui.CollapsingHeader("Material"))
                {
                    int blendingType = materialFlags.HasFlag(ElementMaterialFlags.AdditiveBlending) ? 1 : 0;
                    int filterType = materialFlags.HasFlag(ElementMaterialFlags.LinearFiltering) ? 1 : 0;
                    float width = ImGui.GetWindowSize().X / 2 - 80;
                    ImGui.PushItemWidth(width);
                    if (ImGui.Combo("Blending", ref blendingType, blendingStr, 2))
                    {
                        materialFlags = materialFlags.SetFlag(ElementMaterialFlags.AdditiveBlending, blendingType == 1);
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(width);
                    if (ImGui.Combo("Filtering", ref filterType, filteringStr, 2))
                    {
                        materialFlags = materialFlags.SetFlag(ElementMaterialFlags.LinearFiltering, filterType == 1);
                    }
                    ImGui.BeginDisabled(type != (int)DrawType.Sprite);
                    ImGui.InputInt("Selected Sprite", ref spriteIndex);
                    spriteIndex = Math.Clamp(spriteIndex, -1, 32); //can go over 32 for scu
                    if (ImGui.BeginListBox("##listpatterns", new System.Numerics.Vector2(-1, 160)))
                    {
                        //Draw Pattern selector
                        for (int i = 0; i < selectedCast.SpriteIndices.Length; i++)
                        {
                            int patternIdx = Math.Min(selectedCast.SpriteIndices.Length - 1, (int)selectedCast.SpriteIndices[i]);

                            //Draw button with a different color if its the currently active pattern
                            if (i == spriteIndex) ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(200, 200, 200, 255));
                            // Draw sprite preview if it isnt set to the default square
                            if (patternIdx == -1)
                            {
                                EmptyTextureButton(i);
                            }
                            else
                            {
                                Sprite spriteReference = SpriteHelper.TryGetSprite(selectedCast.SpriteIndices[i]);
                                if (spriteReference != null)
                                {

                                    var uvCoords = spriteReference.GetImGuiUV();

                                    if (spriteReference.Texture.GlTex == null)
                                    {
                                        EmptyTextureButton(i);
                                    }
                                    else
                                    {
                                        ImKunaiTreeNode.SpriteImageButton($"##pattern{i}", spriteReference);
                                    }
                                }
                                if (i != selectedCast.SpriteIndices.Length - 1)
                                    ImGui.SameLine();
                            }
                            if (i == spriteIndex)
                                ImGui.PopStyleColor();
                        }
                        ImGui.EndListBox();
                    }
                    ImGui.EndDisabled();
                }
            }
            selectedCast.Name = name;
            selectedCast.Field00 = (uint)field00;
            selectedCast.Field04 = (uint)type;
            selectedCast.Enabled = enabled;
            info.HideFlag = (uint)(hideflag ? 1 : 0);
            //ADD EDIT FOR HIDE FLAG
            if (mirrorX) materialFlags |= ElementMaterialFlags.MirrorX; else materialFlags &= ~ElementMaterialFlags.MirrorX;
            if (mirrorY) materialFlags |= ElementMaterialFlags.MirrorY; else materialFlags &= ~ElementMaterialFlags.MirrorY;
            selectedCast.Width = (uint)rectSize.X;
            selectedCast.Height = (uint)rectSize.Y;
            selectedCast.TopLeft = topLeftVert / KunaiProject.Instance.ViewportSize;
            selectedCast.TopRight = topRightVert / KunaiProject.Instance.ViewportSize;
            selectedCast.BottomLeft = bottomLeftVert / KunaiProject.Instance.ViewportSize;
            selectedCast.BottomRight = bottomRightVert / KunaiProject.Instance.ViewportSize;
            info.Rotation = rotation;
            selectedCast.Origin = origin;
            info.Translation = translation;
            info.Color = color.Invert().ToSharpNeedleColor();
            info.GradientTopLeft = colorTl.Invert().ToSharpNeedleColor();
            info.GradientTopRight = colorTr.Invert().ToSharpNeedleColor();
            info.GradientBottomLeft = colorBl.Invert().ToSharpNeedleColor();
            info.GradientBottomRight = colorBr.Invert().ToSharpNeedleColor();
            info.SpriteIndex = spriteIndex;
            info.Scale = scale;

            if (inheritPosX) inheritanceFlags |= ElementInheritanceFlags.InheritXPosition; else inheritanceFlags &= ~ElementInheritanceFlags.InheritXPosition;
            if (inheritPosY) inheritanceFlags |= ElementInheritanceFlags.InheritYPosition; else inheritanceFlags &= ~ElementInheritanceFlags.InheritYPosition;
            if (inheritRot) inheritanceFlags |= ElementInheritanceFlags.InheritRotation; else inheritanceFlags &= ~ElementInheritanceFlags.InheritRotation;
            if (inheritCol) inheritanceFlags |= ElementInheritanceFlags.InheritColor; else inheritanceFlags &= ~ElementInheritanceFlags.InheritColor;
            if (inheritScaleX) inheritanceFlags |= ElementInheritanceFlags.InheritScaleX; else inheritanceFlags &= ~ElementInheritanceFlags.InheritScaleX;
            if (inheritScaleY) inheritanceFlags |= ElementInheritanceFlags.InheritScaleY; else inheritanceFlags &= ~ElementInheritanceFlags.InheritScaleY;
            selectedCast.InheritanceFlags = (uint)inheritanceFlags;
            selectedCast.Info = info;
            selectedCast.Field38 = (uint)materialFlags;
            selectedCast.FontName = fontname;
            selectedCast.Text = text;
            selectedCast.Field4C = (uint)BitConverter.ToInt32(BitConverter.GetBytes(kerning / 100), 0);
        }
        public override void Update(KunaiProject in_Proj)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2((ImGui.GetWindowViewport().Size.X / 4.5f) * 3.5f, MenuBarWindow.MenuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(ImGui.GetWindowViewport().Size.X / 4.5f, ImGui.GetWindowViewport().Size.Y - MenuBarWindow.MenuBarHeight), ImGuiCond.Always);
            if (ImGui.Begin("Inspector", MainWindow.WindowFlags))
            {
                if (in_Proj.WorkProjectCsd != null)
                {
                    _fontNames.Clear();
                    foreach (KeyValuePair<string, Font> font in in_Proj.WorkProjectCsd.Project.Fonts)
                    {
                        _fontNames.Add(font.Key);
                    }
                    switch (SelectionType)
                    {
                        case ESelectionType.Scene:
                            {
                                DrawSceneInspector();
                                break;
                            }
                        case ESelectionType.Cast:
                            {
                                DrawCastInspector();
                                break;
                            }
                    }
                }
                ImGui.End();
            }
        }
        internal static void Reset()
        {
            KunaiProject.Instance.SelectionData.SelectedCast = null;
            KunaiProject.Instance.SelectionData.SelectedScene = new();
        }
    }
}