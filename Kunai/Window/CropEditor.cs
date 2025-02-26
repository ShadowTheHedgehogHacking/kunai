using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using Shuriken.Rendering;
using System;
using System.Numerics;

namespace Kunai.Window
{
    public class CropEditor : WindowBase
    {
        public static float ZoomFactor = 1;
        public static bool Enabled = false;
        int selectedIndex = 0;
        int selectedSpriteIndex = 0;
        bool showAllCoords = false;

        void DrawCropBox(ImDrawListPtr drawlist,Vector2 viewportPos, Vector2 imageSize,Sprite in_Sprite, Vector4 in_Color, int in_ID)
        {
            var spr = in_Sprite;
            var colorConv = ImGui.ColorConvertFloat4ToU32(in_Color);
            //unsafe
            //{
            //    drawlist.AddText(ImGuiController.DefaultFont, 20, viewportPos + (new Vector2(spr.OriginalLeft, spr.OriginalTop) * imageSize), colorConv, $"ID: {in_ID}");
            //}
            //ImGui.AddText(drawlist);
            ImGui.AddRect(drawlist, viewportPos + (new Vector2(spr.OriginalLeft, spr.OriginalTop) * imageSize),
                          viewportPos + (new Vector2(spr.OriginalRight, spr.OriginalBottom) * imageSize),
                          colorConv);
        }
        public override void Update(KunaiProject in_Renderer)
        {
            if (in_Renderer.WorkProjectCsd == null)
                return;
            if(Enabled)
            {
                if (ImGui.Begin("Crop", ImGuiWindowFlags.MenuBar))
                {
                    if (ImGui.Button("Add Texture"))
                    {
                        var res = NativeFileDialogSharp.Dialog.FileOpen("dds");
                        if (res.IsOk)
                        {
                            SpriteHelper.AddTexture(new Shuriken.Rendering.Texture(res.Path));
                        }
                    }
                    var size1 = ImGui.GetWindowSize().X / 4;

                    ImGui.Checkbox("Show all crops on texture", ref showAllCoords);
                    ImGui.Separator();
                    if (ImGui.BeginListBox("##texturelist", new System.Numerics.Vector2(size1, -1)))
                    {
                        int idx = 0;
                        var result = ImKunaiControls.TextureSelector(in_Renderer);
                        if (result.TextureIndex != -2)
                            selectedIndex = result.TextureIndex;
                        if (result.SpriteIndex != -2)
                            selectedSpriteIndex = result.SpriteIndex;
                        ImGui.EndListBox();
                    }
                    ImGui.SameLine();
                    Vector2 availableSize = new Vector2(ImGui.GetWindowSize().X / 2, ImGui.GetContentRegionAvail().Y);
                    Vector2 viewportPos = ImGui.GetWindowPos() + ImGui.GetCursorPos();
                    var textureSize = SpriteHelper.TextureList.Textures[selectedIndex].Size;

                    Vector2 imageSize;
                    if (textureSize.X > textureSize.Y)
                        imageSize = new Vector2(availableSize.Y, (textureSize.Y / textureSize.X) * availableSize.Y);
                    else
                        imageSize = new Vector2(availableSize.X, (textureSize.X / textureSize.Y) * availableSize.X);

                    var drawlist = ImGui.GetWindowDrawList();
                    //Texture BG
                    ImGui.AddRectFilled(drawlist, viewportPos, viewportPos + imageSize, ImGui.ColorConvertFloat4ToU32(new Vector4(70 / 255.0f, 70 / 255.0f, 70 / 255.0f, 255)));

                    //Actual texture image
                    if (SpriteHelper.TextureList.Textures[selectedIndex].GlTex != null)
                        ImGui.Image(new ImTextureID(SpriteHelper.TextureList.Textures[selectedIndex].GlTex.Id), imageSize, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));

                    //Show selection boxes for all crops
                    if (showAllCoords)
                    {
                        foreach (var t in SpriteHelper.TextureList.Textures[selectedIndex].Sprites)
                            DrawCropBox(drawlist, viewportPos, imageSize, SpriteHelper.Sprites[t], new Vector4(150, 0, 0, 150), t - 1);

                        var spr = SpriteHelper.Sprites[SpriteHelper.TextureList.Textures[selectedIndex].Sprites[selectedSpriteIndex]];
                        DrawCropBox(drawlist, viewportPos, imageSize, spr, new Vector4(200, 200, 200, 200), selectedSpriteIndex);
                    }
                    else
                    {
                        var spr = SpriteHelper.Sprites[SpriteHelper.TextureList.Textures[selectedIndex].Sprites[selectedSpriteIndex]];
                        DrawCropBox(drawlist, viewportPos, imageSize, spr, new Vector4(200, 200, 200, 200), selectedSpriteIndex);
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
                        ImGui.InputFloat2("Dimension", ref spriteSize, "%.0f");
                        sprite.Start = spriteStart;
                        sprite.Dimensions = spriteSize;
                        ImGui.EndListBox();
                    }

                    ImGui.End();
                }
            }
        }
    }
}
