using Hexa.NET.ImGui;
using Kunai.ShurikenRenderer;
using SharpNeedle.Ninja.Csd;

namespace Kunai.Window
{
    public class HierarchyWindow : WindowBase
    {
        private static TempSearchBox _searchBox = new TempSearchBox();
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
                InspectorWindow.SelectScene(in_Scene.Scene);
                InspectorWindow.SelectCast(in_Cast);
            }
        }
        public static Family CreateNewFamily(Scene in_Parent)
        {
            Family newFamily = new Family();
            newFamily.Scene = in_Parent;
            in_Parent.Families.Add(newFamily);
            return newFamily;
        }
        public static Cast CreateNewCastFromDefault(string in_Name, Cast in_Parent)
        {
            Cast newCast = new Cast();
            newCast.SpriteIndices = new int[32];
            newCast.Parent = in_Parent;
            var info = newCast.Info;
            newCast.Field04 = 1;
            newCast.Name = in_Name;
            info.Scale = new System.Numerics.Vector2(1, 1);
            newCast.TopLeft = new System.Numerics.Vector2(-0.1f, 0);
            newCast.BottomLeft = new System.Numerics.Vector2(-0.1f, 0.1f);
            newCast.TopRight = new System.Numerics.Vector2(0.1f, 0);
            newCast.BottomRight = new System.Numerics.Vector2(0.1f, 0.1f);
            return newCast;
        }
        private static void DrawSceneNode(SVisibilityData.SNode in_VisNode)
        {
            bool selectedNode = false;
            bool selectedScene = false;
            if (ImguiControls.CollapsingHeaderVisibility($"#{in_VisNode.Node.Key}", ref in_VisNode.Active, ref selectedNode))
            {

                foreach (var inNode in in_VisNode.Nodes)
                    DrawSceneNode(inNode);
                foreach (var g in in_VisNode.Scene)
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
                    if(ImGui.BeginPopupContextItem())
                    {
                        if(ImGui.Selectable("Create new cast"))
                        {
                            Family newFam = CreateNewFamily(g.Scene.Value);
                            Cast newCast = CreateNewCastFromDefault("New_Cast", null);
                            newFam.Add(newCast);
                            g.Casts.Add(new SVisibilityData.SCast(newCast));
                        }
                        ImGui.EndPopup();
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
        public override void Update(ShurikenRenderHelper in_Proj)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, MenuBarWindow.MenuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(ImGui.GetWindowViewport().Size.X / 4.5f, ImGui.GetWindowViewport().Size.Y), ImGuiCond.Always);
            if (ImGui.Begin("Hierarchy", MainWindow.WindowFlags))
            {
                ImGui.BeginDisabled(true);
                _searchBox.Render();
                ImGui.EndDisabled();
                if (in_Proj.WorkProjectCsd != null)
                {
                    foreach (var f in MainWindow.Renderer.VisibilityData.Nodes)
                    {
                        DrawSceneNode(f);
                    }

                }
            }
            ImGui.End();
        }
    }
}
