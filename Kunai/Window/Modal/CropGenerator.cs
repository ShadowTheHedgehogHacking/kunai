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
    public class CropGenerator : ModalWindow
    {
        int m_CutType;
        Vector2 m_CutParam;
        Vector2 m_ModalSize = new Vector2(500, 160);
        public int in_TextureIndex;
        public override void Setup()
        {
            name = "##cropgenerator";
            size = m_ModalSize;
        }
        public override void DrawContents()
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
                SetEnabled(false);
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                SetEnabled(false);
                ImGui.CloseCurrentPopup();
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
