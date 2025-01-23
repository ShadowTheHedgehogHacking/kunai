using Kunai.ShurikenRenderer;
using ImGuiNET;
using SharpNeedle.Ninja.Csd;
using Shuriken.Rendering;
using Sprite = Shuriken.Rendering.Sprite;

namespace Kunai.Window
{
    public static class InspectorWindow
    {
        public enum ESelectionType
        {
            None,
            Scene,
            Family,
            Cast
        }
        public static ESelectionType eSelectionType;
        public static Cast SelectedCast;
        public static KeyValuePair<string, Scene> SelectedScene;
        public static Family SelectedFamily;
        public static void SelectCast(Cast in_Cast)
        {
            SelectedCast = in_Cast;
            eSelectionType = ESelectionType.Cast;
        }
        public static void SelectScene(KeyValuePair<string, Scene> in_Cast)
        {
            SelectedScene = in_Cast;
            eSelectionType = ESelectionType.Scene;
        }
        public static void DrawSceneInspector()
        {
            string name = SelectedScene.Key;
            var vers = SelectedScene.Value.Version;
            var priority = (int)SelectedScene.Value.Priority;
            ImGui.InputText("Name", ref name, 256);
            ImGui.InputInt("Version", ref vers);    
            ImGui.InputInt("Priority", ref priority);    
            ImGui.InputInt("Priority", ref priority);    
            ImGui.Text(SelectedScene.Key);
        }
        public static void DrawCastInspector()
        {
            string[] typeStrings = { "No Draw", "Sprite", "Font" };
            var materialFlags = (ElementMaterialFlags)SelectedCast.Field38;
            var info = SelectedCast.Info;
            var inheritanceFlags = (ElementInheritanceFlags)SelectedCast.InheritanceFlags.Value;


            string name = SelectedCast.Name;
            int field00 = (int)SelectedCast.Field00;
            var type = (int)SelectedCast.Field04;
            var enabled = SelectedCast.Enabled;
            var hideflag = info.HideFlag == 1;
            var mirrorX = materialFlags.HasFlag(ElementMaterialFlags.MirrorX);
            var mirrorY = materialFlags.HasFlag(ElementMaterialFlags.MirrorY);
            var sizeX = (int)SelectedCast.Width;
            var sizeY = (int)SelectedCast.Height;
            var topLeftVert = SelectedCast.TopLeft;
            var topRightVert = SelectedCast.TopRight;
            var bottomLeftVert = SelectedCast.BottomLeft;
            var bottomRightVert = SelectedCast.BottomRight;
            var rotation = info.Rotation;
            var origin = SelectedCast.Origin;
            var translation = info.Translation;
            var color = info.Color.ToVec4();
            var colorTL = info.GradientTopLeft.ToVec4();
            var colorTR = info.GradientTopRight.ToVec4();
            var colorBL = info.GradientBottomLeft.ToVec4();
            var colorBR = info.GradientBottomRight.ToVec4();

            bool inheritPosX = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritXPosition);
            bool inheritPosY = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritYPosition);
            bool inheritRot = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritRotation);
            bool inheritCol = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritColor);
            bool inheritScaleX = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleX);
            bool inheritScaleY = inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleY);
            int spriteIndex = (int)info.SpriteIndex;


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
                ImGui.Checkbox("H", ref mirrorX);
                ImGui.SameLine();
                ImGui.Checkbox("V", ref mirrorY);
                ImGui.EndGroup();

                ImGui.SeparatorText("Rect Size");
                ImGui.InputInt("W", ref sizeX);
                ImGui.SameLine();
                ImGui.InputInt("H", ref sizeY);

                ImGui.SeparatorText("Vertices");

                ImGui.InputFloat2("Top Left", ref topLeftVert);
                ImGui.InputFloat2("Top Right", ref topRightVert);
                ImGui.InputFloat2("Bottom Left", ref bottomLeftVert);
                ImGui.InputFloat2("Bottom Right", ref bottomRightVert);

            }
            if (ImGui.CollapsingHeader("Transform"))
            {
                ImGui.InputFloat("Rotation", ref rotation);
                ImGui.InputFloat2("Origin", ref origin);
                ImGui.InputFloat2("Translation", ref translation);
            }
            if (ImGui.CollapsingHeader("Color"))
            {
                ImGui.ColorEdit4("Color", ref color);
                ImGui.ColorEdit4("Top Left", ref colorTL);
                ImGui.ColorEdit4("Top Right", ref colorTR);
                ImGui.ColorEdit4("Bottom Left", ref colorBL);
                ImGui.ColorEdit4("Bottom Right", ref colorBR);
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
            if (ImGui.CollapsingHeader("Material"))
            {
                ImGui.BeginDisabled(type != (int)DrawType.Sprite);
                ImGui.InputInt("Selected Sprite", ref spriteIndex);
                spriteIndex = Math.Clamp(spriteIndex, -1, 32); //can go over 32 for scu

                //Draw Pattern selector
                for (int i = 0; i < SelectedCast.SpriteIndices.Length; i++)
                {
                    int patternIdx = Math.Min(SelectedCast.SpriteIndices.Length - 1, (int)SelectedCast.SpriteIndices[i]);

                    //Draw button with a different color if its the currently active pattern
                    if (i == spriteIndex) ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(200, 200, 200, 255));
                    // Draw sprite preview if it isnt set to the default square
                    if (patternIdx == -1)
                    {
                        ImGui.Button($"##pattern{i}", new System.Numerics.Vector2(55, 55));
                        ImGui.SameLine();
                    }
                    else
                    {
                        Sprite spriteReference = SpriteHelper.TryGetSprite(SelectedCast.SpriteIndices[i]);
                        if (spriteReference == null) continue;

                        ShurikenRenderer.Vector2 uvTL = new ShurikenRenderer.Vector2(
                        spriteReference.Start.X / spriteReference.Texture.Width,
                        -(spriteReference.Start.Y / spriteReference.Texture.Height));

                        ShurikenRenderer.Vector2 uvBR = uvTL + new ShurikenRenderer.Vector2(
                        spriteReference.Dimensions.X / spriteReference.Texture.Width,
                        -(spriteReference.Dimensions.Y / spriteReference.Texture.Height));

                        //Draw sprite
                        ImGui.ImageButton($"##pattern{i}", (IntPtr)spriteReference.Texture.GlTex.ID, new System.Numerics.Vector2(50, 50), uvTL, uvBR);

                        if (i != SelectedCast.SpriteIndices.Length - 1)
                            ImGui.SameLine();
                    }
                    if (i == spriteIndex)
                        ImGui.PopStyleColor();
                }
                ImGui.EndDisabled();
            }
            SelectedCast.Name = name;
            SelectedCast.Field00 = (uint)field00;
            SelectedCast.Field04 = (uint)type;
            SelectedCast.Enabled = enabled;
            info.HideFlag = (uint)(hideflag ? 1 : 0);
            //ADD EDIT FOR HIDE FLAG
            if (mirrorX) materialFlags |= ElementMaterialFlags.MirrorX; else materialFlags &= ~ElementMaterialFlags.MirrorX;
            if (mirrorY) materialFlags |= ElementMaterialFlags.MirrorY; else materialFlags &= ~ElementMaterialFlags.MirrorY;
            SelectedCast.Width = (uint)sizeX;
            SelectedCast.Height = (uint)sizeY;
            SelectedCast.TopLeft = topLeftVert;
            SelectedCast.TopRight = topRightVert;
            SelectedCast.BottomLeft = bottomLeftVert;
            SelectedCast.BottomRight = bottomRightVert;
            info.Rotation = rotation;
            SelectedCast.Origin = origin;
            info.Translation = translation;
            info.Color = color.ToSharpNeedleColor();
            info.GradientTopLeft = colorTL.ToSharpNeedleColor();
            info.GradientTopRight = colorTR.ToSharpNeedleColor();
            info.GradientBottomLeft = colorBL.ToSharpNeedleColor();
            info.GradientBottomRight = colorBR.ToSharpNeedleColor();
            info.SpriteIndex = spriteIndex;

            if (inheritPosX) inheritanceFlags |= ElementInheritanceFlags.InheritXPosition; else inheritanceFlags &= ~ElementInheritanceFlags.InheritXPosition;
            if (inheritPosY) inheritanceFlags |= ElementInheritanceFlags.InheritYPosition; else inheritanceFlags &= ~ElementInheritanceFlags.InheritYPosition;
            if (inheritRot) inheritanceFlags |= ElementInheritanceFlags.InheritRotation; else inheritanceFlags &= ~ElementInheritanceFlags.InheritRotation;
            if (inheritCol) inheritanceFlags |= ElementInheritanceFlags.InheritColor; else inheritanceFlags &= ~ElementInheritanceFlags.InheritColor;
            if (inheritScaleX) inheritanceFlags |= ElementInheritanceFlags.InheritScaleX; else inheritanceFlags &= ~ElementInheritanceFlags.InheritScaleX;
            if (inheritScaleY) inheritanceFlags |= ElementInheritanceFlags.InheritScaleY; else inheritanceFlags &= ~ElementInheritanceFlags.InheritScaleY;
            SelectedCast.InheritanceFlags = (uint)inheritanceFlags;
            SelectedCast.Info = info;
            SelectedCast.Field38 = (uint)materialFlags;
        }

        public static void Render(CsdProject in_Proj)
        {
            if (ImGui.Begin("Inspector"))
            {
                switch (eSelectionType)
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
}
