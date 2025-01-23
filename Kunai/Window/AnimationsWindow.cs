

using Hexa.NET.ImGui;
using Kunai.ShurikenRenderer;

namespace Kunai.Window
{
    public static class AnimationsWindow
    {

        public static void Render(ShurikenRenderHelper in_Renderer)
        {
            var size1 = ImGui.GetWindowViewport().Size.X / 4.5f;
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(size1, ImGui.GetWindowViewport().Size.Y / 1.5f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(size1 * 2.5f, ImGui.GetWindowViewport().Size.Y/3), ImGuiCond.Always);
            if (ImGui.Begin("Animations", MainWindow.flags))
            {
                if (InspectorWindow.SelectedScene.Value != null)
                {
                    if (ImGui.BeginListBox("##animlist"))
                    {
                        var scene = in_Renderer.sVisibilityData.GetScene(InspectorWindow.SelectedScene.Value);
                        foreach (var item in scene.Animation)
                        {
                            bool selected = false;
                            if (ImguiControls.CollapsingHeaderVisibility(item.Motion.Key, ref item.Active, ref selected, true))
                            {
                                foreach (SharpNeedle.Ninja.Csd.Motions.FamilyMotion item2 in item.Motion.Value.FamilyMotions)
                                {
                                    foreach (var item3 in item2.CastMotions)
                                    {

                                        ImGui.Text(item3.Cast.Name);
                                    }
                                }
                                ImGui.TreePop();
                            }
                        }
                        ImGui.EndListBox();
                    }
                }
                ImGui.End();
            }
        }
    }
}
