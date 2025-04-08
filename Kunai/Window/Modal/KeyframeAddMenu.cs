using Hexa.NET.ImGui;
using SharpNeedle.Framework.Ninja.Csd.Motions;
using System;
using System.Numerics;

namespace Kunai.Window.Modal
{
    public class KeyframeAddMenu : ModalWindow
    {
        Vector2 m_ModalSize = new Vector2(500, 400);
        public int in_TextureIndex;
        public CastMotion motion;
        int m_SelectedProp;
        public override void Setup()
        {
            name = "##keyframeadd";
            size = m_ModalSize;
        }
        public override void DrawContents()
        {
            var size = ImGui.GetContentRegionAvail();
            if (ImGui.BeginListBox("##keyframeadd", new Vector2(-1, m_ModalSize.Y - 80)))
            {
                for (int i = 0; i < 12; i++)
                {
                    var info = AnimationsWindow.GetDisplayNameAndIcon((KeyProperty)i);

                    if (ImKunai.AnimationTreeNode(info))
                    {
                        m_SelectedProp = i;
                    }
                }
                ImGui.EndListBox();
            }
            ImGui.Separator();
            if (ImGui.Button("Execute"))
            {
                var list = new KeyFrameList();
                list.Property = (KeyProperty)m_SelectedProp;
                motion.Add(list);
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
    }
}