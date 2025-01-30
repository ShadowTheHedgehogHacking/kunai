using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using System;
using System.Numerics;

namespace Kunai.Window
{
    public class CropEditor : WindowBase
    {
        public static float ZoomFactor = 1;
        int selectedIndex = 0;
        int selectedSpriteIndex = 0;
        bool showAllCoords = false;
        public override void Update(KunaiProject in_Renderer)
        {
            if (in_Renderer.WorkProjectCsd == null)
                return;
            if (ImGui.Begin("Crop"))
            {
                var size1 = ImGui.GetWindowSize().X / 4;

                ImGui.Checkbox("Show all crops on texture", ref showAllCoords);
                ImGui.Separator();
                if (ImGui.BeginListBox("##texturelist", new System.Numerics.Vector2(size1, -1)))
                {
                    int idx = 0;
                    foreach (var t in in_Renderer.WorkProjectCsd.Textures)
                    {
                        if(ImGui.TreeNode(t.Name))
                        {
                            int idx2 = 0;
                            foreach(var s in SpriteHelper.TextureList.Textures[idx].Sprites)
                            {
                                Shuriken.Rendering.Sprite spr = SpriteHelper.Sprites[s];

                                if(ImKunaiTreeNode.SpriteImageButton($"##crop{idx2}", spr))
                                {
                                    selectedSpriteIndex = idx2;
                                }
                                ImGui.SameLine();
                                ImGui.Text($"Crop ({idx2})");
                                idx2++;
                            }

                            ImGui.TreePop();
                        }
                        idx++;
                    }
                    ImGui.EndListBox();
                }
                ImGui.SameLine();
                Vector2 availableSize = new Vector2(ImGui.GetWindowSize().X / 2, ImGui.GetContentRegionAvail().Y);
                Vector2 viewportPos = ImGui.GetWindowPos() + ImGui.GetCursorPos();
                var textureSize = SpriteHelper.TextureList.Textures[selectedIndex].Size;
                Vector2 imageSize;
                if(textureSize.X > textureSize.Y)
                {
                    imageSize = new Vector2(availableSize.Y, (textureSize.Y / textureSize.X) * availableSize.Y);
                }
                else
                {
                    imageSize = new Vector2(availableSize.X, (textureSize.X / textureSize.Y) * availableSize.X);
                }
                if(SpriteHelper.TextureList.Textures[selectedIndex].GlTex != null)
                ImGui.Image(new ImTextureID(SpriteHelper.TextureList.Textures[selectedIndex].GlTex.Id), imageSize, new System.Numerics.Vector2(0,1), new System.Numerics.Vector2(1, 0));
                var drawlist = ImGui.GetWindowDrawList();
                if(showAllCoords)
                {
                    foreach (var t in SpriteHelper.TextureList.Textures[selectedIndex].Sprites)
                    {
                        var spr = SpriteHelper.Sprites[t];
                        ImGui.AddRect(drawlist, viewportPos + (new Vector2(spr.OriginalLeft, spr.OriginalTop) * imageSize), viewportPos + (new Vector2(spr.OriginalRight, spr.OriginalBottom) * imageSize), ImGui.ColorConvertFloat4ToU32(new Vector4(255, 255, 255, 255)));
                        //break;
                    }
                }
                else
                {
                    var spr = SpriteHelper.Sprites[SpriteHelper.TextureList.Textures[selectedIndex].Sprites[selectedSpriteIndex]];
                    ImGui.AddRect(drawlist, viewportPos + (new Vector2(spr.OriginalLeft, spr.OriginalTop) * imageSize), viewportPos + (new Vector2(spr.OriginalRight, spr.OriginalBottom) * imageSize), ImGui.ColorConvertFloat4ToU32(new Vector4(255, 255, 255, 255)));


                }
                ImGui.SameLine();
                if (ImGui.BeginListBox("##texturelist2", new System.Numerics.Vector2(size1, -1)))
                {
                    var texture = SpriteHelper.TextureList.Textures[selectedIndex];
                    var sprite = SpriteHelper.Sprites[texture.Sprites[selectedSpriteIndex]];
                    Vector2 spriteStart = sprite.Start;
                    Vector2 spriteSize = sprite.Dimensions;
                    ImGui.SeparatorText("Texture Info");
                    ImGui.Text($"Name: {texture.Name}");
                    ImGui.Text($"Width: {texture.Width}");
                    ImGui.Text($"Height: {texture.Height}");
                    ImGui.SeparatorText("Crop");
                    ImGui.InputFloat2("Position", ref spriteStart, "%.0f");
                    ImGui.InputFloat2("Dimension", ref spriteSize,"%.0f");
                    sprite.Start = spriteStart;
                    sprite.Dimensions = spriteSize;
                    ImGui.EndListBox();
                }

                ImGui.End();
            }
        }
    }
}
