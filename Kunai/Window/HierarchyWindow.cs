using Hexa.NET.ImGui;
using Kunai.ShurikenRenderer;
using SharpNeedle.Ninja.Csd;

namespace Kunai.Window
{
    public static class HierarchyWindow
    {

        private static void RecursiveCastWidget(SVisibilityData.SScene in_Scene, Cast in_Cast)
        {
            bool selectedcast = false;
            var vis = in_Scene.GetVisibility(in_Cast);
            if (ImguiControls.CollapsingHeaderVisibility(in_Cast.Name, ref vis.Active, ref selectedcast, in_Cast.Children.Count > 0))
            {
                for (int x = 0; x < in_Cast.Children.Count; x++)
                {
                    RecursiveCastWidget(in_Scene, in_Cast.Children[x]);
                    //for (int i = 0; i < in_Cast.Children[x].Children.Count; i++)
                    //{
                    //    RecursiveCastWidget(in_Scene, in_Cast.Children[x].Children[i]);
                    //}
                }
                ImGui.TreePop();
            }
            if (selectedcast)
            {
                InspectorWindow.SelectCast(in_Cast);
            }
        }
        public static void Render(CsdProject in_Proj)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, MenuBarWindow.menuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(ImGui.GetWindowViewport().Size.X / 4.5f, ImGui.GetWindowViewport().Size.Y), ImGuiCond.Always);
            if (ImGui.Begin("Hierarchy", MainWindow.flags))
            {
                if (in_Proj != null)
                {
                    foreach (SVisibilityData.SNode f in MainWindow.renderer.visibilityData.Nodes)
                    {
                        bool selectedNode = false;
                        bool selectedScene = false;
                        if (ImguiControls.CollapsingHeaderVisibility(f.Node.Key, ref f.Active, ref selectedNode))
                        {
                            foreach (var g in f.Scene)
                            {
                                if (ImguiControls.CollapsingHeaderVisibility(g.Scene.Key, ref g.Active, ref selectedScene))
                                {
                                    for (int i = 0; i < g.Scene.Value.Families.Count; i++)
                                    {
                                        var family = g.Scene.Value.Families[i];

                                        var castFamilyRoot = family.Casts[0];
                                        RecursiveCastWidget(g, castFamilyRoot);

                                    }
                                    ImGui.TreePop();
                                }
                                if (selectedScene)
                                {
                                    InspectorWindow.SelectScene(g.Scene);
                                }
                            }
                            ImGui.TreePop();
                        }
                        //if (ImGui.TreeNodeEx(f.Node.Key))
                        //{
                        //    foreach (var g in f.Scene)
                        //    {
                        //        if (ImGui.TreeNodeEx(f.Node.Key))
                        //        {
                        //            ImGui.Checkbox($"##{g.Scene.Key}togg", ref g.Active);
                        //        ImGui.SameLine();
                        //        ImGui.Text(g.Scene.Key);
                        //    }
                        //    ImGui.TreePop();
                        //}
                    }

                }
            }
            ImGui.End();
        }
    }
}
