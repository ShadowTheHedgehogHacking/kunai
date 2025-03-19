using Hexa.NET.ImGui;
using Kunai.ShurikenRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kunai.Modal
{
    public static class CropGenerator
    {
        static bool ms_MCutMenuOpen = false;
        static int ms_MCutType;
        static Vector2 ms_MCutParam;
        static Vector2 ms_MWindowSize = new Vector2(500, 160);

        public static void Activate()
        {
            ms_MCutMenuOpen = true;
        }
        public static void Draw(int in_TextureIndex)
        {
            if (ms_MCutMenuOpen)
            {
                ImGui.OpenPopup("##generatecrops");

                // Calculate centered position
                var viewport = ImGui.GetMainViewport();
                Vector2 centerPos = new Vector2(
                    viewport.WorkPos.X + (viewport.WorkSize.X - ms_MWindowSize.X) * 0.5f,
                    viewport.WorkPos.Y + (viewport.WorkSize.Y - ms_MWindowSize.Y) * 0.5f
                );
                ImGui.SetNextWindowPos(centerPos);
                ImGui.SetNextWindowSize(ms_MWindowSize);
                if (ImGui.BeginPopupModal("##generatecrops", ref ms_MCutMenuOpen, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
                {
                    var texture = SpriteHelper.Textures[in_TextureIndex];
                    ImGui.Combo("Type", ref ms_MCutType, ["Size", "Count"], 2);

                    ImGui.InputFloat2(ms_MCutType == 0 ? "Individual Size" : "Grid Count", ref ms_MCutParam);
                    ImGui.Separator();

                    //Calculate estimated crop count
                    Vector2 estimate = ms_MCutType == 0 ? texture.Size / ms_MCutParam : ms_MCutParam;
                    float estimateCount = (int)(estimate.Y * estimate.X);
                    if (estimateCount < 0 || estimateCount > 256)
                        estimateCount = 0;

                    ImGui.Text($"{estimateCount} crops will be generated");
                    ImGui.Separator();
                    if (ImGui.Button("Execute"))
                    {
                        switch (ms_MCutType)
                        {
                            case 0:
                                {
                                    CreateCropGrid(in_TextureIndex, ms_MCutParam);
                                    break;
                                }
                            case 1:
                                {
                                    CreateCropGrid(in_TextureIndex, texture.Size / ms_MCutParam);
                                    break;
                                }
                        }
                        ms_MCutMenuOpen = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        ms_MCutMenuOpen = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
            }
        }

        static void CreateCropGrid(int in_TexIdx, Vector2 in_IndividualSize)
        {
            var texture = SpriteHelper.Textures[in_TexIdx];
            Vector2 count = new Vector2(
                MathF.Floor(texture.Size.X / in_IndividualSize.X),
                MathF.Floor(texture.Size.Y / in_IndividualSize.Y)
            );

            // Arbitrary limit
            if (count.X > 256 || count.Y > 256) return;
            if (count.X == 0 || count.Y == 0) return;

            for (int y = 0; y < count.Y; y++)
            {
                for (int x = 0; x < count.X; x++)
                {
                    // Calculate position in [0,1] range
                    var position = new Vector2(
                        (x * in_IndividualSize.X) / texture.Size.X,
                        (y * in_IndividualSize.Y) / texture.Size.Y
                    );

                    // Calculate size in [0,1] range
                    var newSize = new Vector2(
                        in_IndividualSize.X / texture.Size.X,
                        in_IndividualSize.Y / texture.Size.Y
                    );

                    // Create and add sprite with normalized position and size
                    SpriteHelper.CreateSprite(texture, position, newSize);
                }
            }
        }
    }
}
