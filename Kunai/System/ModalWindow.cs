using Hexa.NET.ImGui;
using System.Numerics;

namespace Kunai
{
    public class ModalWindow
    {
        public string name;
        public Vector2 size;
        private bool isOpen;
        private bool wasOpen;
        public void SetEnabled(bool enabled)
        {
            isOpen = enabled;
        }
        public virtual void DrawContents()
        {

        }
        public virtual void Setup()
        {

        }
        public void Draw()
        {
            if (isOpen)
            {
                Setup();
                if(!wasOpen)
                {
                    ImGui.OpenPopup(name);
                }

                // Calculate centered position
                var viewport = ImGui.GetMainViewport();
                Vector2 centerPos = new Vector2(
                    viewport.WorkPos.X + (viewport.WorkSize.X - size.X) * 0.5f,
                    viewport.WorkPos.Y + (viewport.WorkSize.Y - size.Y) * 0.5f
                );
                ImGui.SetNextWindowPos(centerPos);
                ImGui.SetNextWindowSize(size);
                if (ImGui.BeginPopupModal(name, ref isOpen, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
                {
                    DrawContents();
                    ImGui.EndPopup();
                }
            }
            wasOpen = isOpen;
        }
    }
}

