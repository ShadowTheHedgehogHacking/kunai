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
        static bool m_CutMenuOpen = false;
        static int m_CutType;
        static Vector2 m_CutParam;
        static Vector2 m_WindowSize = new Vector2(500, 160);

        public static void Activate()
        {
            m_CutMenuOpen = true;
        }
        public static void Draw(int in_TextureIndex)
        {
            if (m_CutMenuOpen)
            {
                ImGui.OpenPopup("##generatecrops");

                // Calculate centered position
                var viewport = ImGui.GetMainViewport();
                Vector2 centerPos = new Vector2(
                    viewport.WorkPos.X + (viewport.WorkSize.X - m_WindowSize.X) * 0.5f,
                    viewport.WorkPos.Y + (viewport.WorkSize.Y - m_WindowSize.Y) * 0.5f
                );
                ImGui.SetNextWindowPos(centerPos);
                ImGui.SetNextWindowSize(m_WindowSize);
                if (ImGui.BeginPopupModal("##generatecrops", ref m_CutMenuOpen, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
                {
                    var texture = SpriteHelper.Textures[in_TextureIndex];
                    ImGui.Combo("Type", ref m_CutType, ["Size", "Count"], 2);

                    ImGui.InputFloat2(m_CutType == 0 ? "Individual Size" : "Grid Count", ref m_CutParam);
                    ImGui.Separator();

                    //Calculate estimated crop count
                    Vector2 estimate = m_CutType == 0 ? texture.Size / m_CutParam : m_CutParam;
                    float estimateCount = (int)(estimate.Y * estimate.X);
                    if (estimateCount < 0 || estimateCount > 256)
                        estimateCount = 0;

                    ImGui.Text($"{estimateCount} crops will be generated");
                    ImGui.Separator();
                    if (ImGui.Button("Execute"))
                    {
                        switch (m_CutType)
                        {
                            case 0:
                                {
                                    CreateCropGrid(in_TextureIndex, m_CutParam);
                                    break;
                                }
                            case 1:
                                {
                                    CreateCropGrid(in_TextureIndex, texture.Size / m_CutParam);
                                    break;
                                }
                        }
                        m_CutMenuOpen = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        m_CutMenuOpen = false;
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
