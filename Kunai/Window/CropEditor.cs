using HekonrayBase;
using HekonrayBase.Base;
using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using Shuriken.Rendering;
using System;
using System.Numerics;

namespace Kunai.Window
{
    public class CropEditor : Singleton<CropEditor>, IWindow
    {
        public static float ZoomFactor = 1;
        public static bool Enabled = false;
        int m_SelectedIndex = 0;
        int m_SelectedSpriteIndex = 0;
        bool m_ShowAllCoords = false;

        void DrawCropBox(ImDrawListPtr in_Drawlist,Vector2 in_ViewportPos, Vector2 in_ImageSize,Sprite in_Sprite, Vector4 in_Color, int in_Id)
        {
            var spr = in_Sprite;
            var colorConv = ImGui.ColorConvertFloat4ToU32(in_Color);
            //unsafe
            //{
            //    drawlist.AddText(ImGuiController.DefaultFont, 20, viewportPos + (new Vector2(spr.OriginalLeft, spr.OriginalTop) * imageSize), colorConv, $"ID: {in_ID}");
            //}
            //ImGui.AddText(drawlist);
            ImGui.AddRect(in_Drawlist, in_ViewportPos + (new Vector2(spr.OriginalLeft, spr.OriginalTop) * in_ImageSize),
                          in_ViewportPos + (new Vector2(spr.OriginalRight, spr.OriginalBottom) * in_ImageSize),
                          colorConv);
        }

        public void OnReset(IProgramProject in_Renderer)
        {
            throw new NotImplementedException();
        }

        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (KunaiProject)in_Renderer;
            if (renderer.WorkProjectCsd == null)
                return;
            if (Enabled)
            {
                if (ImGui.Begin("Crop"))
                {
                    if (ImGui.Button("Add Texture"))
                    {
                        var res = NativeFileDialogSharp.Dialog.FileOpen("dds");
                        if (res.IsOk)
                        {
                            if (!SpriteHelper.DoesTextureExist(res.Path))
                            {
                                SpriteHelper.AddTexture(new Texture(res.Path));
                            }
                            else
                            {
                                Application.ShowMessageBoxCross("Error", "A texture with this exact name already exists!\nPlease rename the target texture and try again.");
                            }
                        }
                    }
                    var size1 = ImGui.GetWindowSize().X / 4;

                    ImGui.Checkbox("Show all crops on texture", ref m_ShowAllCoords);
                    ImGui.Separator();
                    if (ImGui.BeginListBox("##texturelist", new System.Numerics.Vector2(size1, -1)))
                    {
                        int idx = 0;
                        var result = ImKunai.TextureSelector(renderer, true);
                        if (result.TextureIndex != -2)
                            m_SelectedIndex = result.TextureIndex;
                        if (result.SpriteIndex != -2)
                            m_SelectedSpriteIndex = result.SpriteIndex;
                        ImGui.EndListBox();
                    }
                    ImGui.SameLine();
                    Vector2 availableSize = new Vector2(ImGui.GetWindowSize().X / 2, ImGui.GetContentRegionAvail().Y);
                    Vector2 viewportPos = ImGui.GetWindowPos() + ImGui.GetCursorPos();
                    var textureSize = SpriteHelper.Textures[m_SelectedIndex].Size;

                    Vector2 imageSize;
                    if (textureSize.X > textureSize.Y)
                        imageSize = new Vector2(availableSize.Y, (textureSize.Y / textureSize.X) * availableSize.Y);
                    else
                        imageSize = new Vector2(availableSize.X, (textureSize.X / textureSize.Y) * availableSize.X);

                    var drawlist = ImGui.GetWindowDrawList();
                    //Texture BG
                    ImGui.AddRectFilled(drawlist, viewportPos, viewportPos + imageSize, ImGui.ColorConvertFloat4ToU32(new Vector4(70 / 255.0f, 70 / 255.0f, 70 / 255.0f, 255)));

                    //Actual texture image
                    if (SpriteHelper.Textures[m_SelectedIndex].GlTex != null)
                        ImGui.Image(new ImTextureID(SpriteHelper.Textures[m_SelectedIndex].GlTex.Id), imageSize, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));

                    //Show selection boxes for all crops
                    if (m_ShowAllCoords)
                    {
                        foreach (var t in SpriteHelper.Textures[m_SelectedIndex].Sprites)
                            DrawCropBox(drawlist, viewportPos, imageSize, SpriteHelper.Sprites[t], new Vector4(150, 0, 0, 150), t - 1);

                        var spr = SpriteHelper.Sprites[SpriteHelper.Textures[m_SelectedIndex].Sprites[m_SelectedSpriteIndex]];
                        DrawCropBox(drawlist, viewportPos, imageSize, spr, new Vector4(200, 200, 200, 200), m_SelectedSpriteIndex);
                    }
                    else
                    {
                        var spr = SpriteHelper.Sprites[SpriteHelper.Textures[m_SelectedIndex].Sprites[m_SelectedSpriteIndex]];
                        DrawCropBox(drawlist, viewportPos, imageSize, spr, new Vector4(200, 200, 200, 200), m_SelectedSpriteIndex);
                    }
                    ImGui.SameLine();
                    if (ImGui.BeginListBox("##texturelist2", new System.Numerics.Vector2(size1, -1)))
                        {
                            var texture = SpriteHelper.Textures[m_SelectedIndex];
                            var sprite = SpriteHelper.Sprites[texture.Sprites[m_SelectedSpriteIndex]];
                            Vector2 spriteStart = sprite.Start;
                            Vector2 spriteSize = sprite.Dimensions;
                            ImGui.SeparatorText("Texture Info");
                            ImGui.Text($"Name: {texture.Name}");
                            ImGui.Text($"Width: {texture.Width}");
                            ImGui.Text($"Height: {texture.Height}");
                            ImGui.SeparatorText("Crop");
                            ImGui.InputFloat2("Position", ref spriteStart, "%.0f");
                            ImGui.InputFloat2("Dimension", ref spriteSize, "%.0f");
                            sprite.Start = spriteStart;
                            sprite.Dimensions = spriteSize;
                            sprite.Recalculate();
                            ImGui.EndListBox();
                        }

                    ImGui.End();
                }
            }
        }
    }
}
