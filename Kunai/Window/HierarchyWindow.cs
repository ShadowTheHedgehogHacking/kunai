using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using SharpNeedle.Ninja.Csd;
using System;
using System.Numerics;
using static Kunai.Window.ImKunaiTreeNode;

namespace Kunai.Window
{
    public class HierarchyWindow : WindowBase
    {
        private static TempSearchBox _searchBox = new TempSearchBox();
        private static void RecursiveCastWidget(SVisibilityData.SScene in_Scene, Cast in_Cast)
        {
            bool selectedcast = false;
            var vis = in_Scene.GetVisibility(in_Cast);
            SIconData icon = new();
            switch(in_Cast.GetDrawType())
            {
                case Shuriken.Rendering.DrawType.Sprite:
                    {
                        icon = NodeIconResource.CastSprite;
                        break;
                    }
                case Shuriken.Rendering.DrawType.None:
                    {
                        icon = NodeIconResource.CastNull;
                        break;
                    }
                case Shuriken.Rendering.DrawType.Font:
                    {
                        icon = NodeIconResource.CastFont;
                        break;
                    }
            }
            //Casts
            if (ImKunaiTreeNode.VisibilityNode(in_Cast.Name, ref vis.Active, ref selectedcast, in_ShowArrow: in_Cast.Children.Count > 0, in_Icon: icon))
            {
                for (int x = 0; x < in_Cast.Children.Count; x++)
                {
                    RecursiveCastWidget(in_Scene, in_Cast.Children[x]);
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
            for (int i = 0; i < 32; i++)
                newCast.SpriteIndices[i] = -1;
            newCast.Parent = in_Parent;
            var info = newCast.Info;
            newCast.Field04 = 1;
            newCast.Name = in_Name;
            info.Scale = new System.Numerics.Vector2(1, 1);
            info.SpriteIndex = -1;
            newCast.Enabled = true;
            newCast.TopLeft = new System.Numerics.Vector2(-0.02f, -0.03f);
            newCast.BottomLeft = new System.Numerics.Vector2(-0.02f, 0.03f);
            newCast.TopRight = new System.Numerics.Vector2(0.02f, -0.03f);
            newCast.BottomRight = new System.Numerics.Vector2(0.02f, 0.03f);
            info.Color = Vector4.One.ToSharpNeedleColor();
            info.GradientTopLeft = Vector4.One.ToSharpNeedleColor();
            info.GradientBottomLeft = Vector4.One.ToSharpNeedleColor();
            info.GradientTopRight = Vector4.One.ToSharpNeedleColor();
            info.GradientBottomRight = Vector4.One.ToSharpNeedleColor();
            newCast.Info = info;
            return newCast;
        }
        public static void AddCastOption(SVisibilityData.SScene in_Scene)
        {
            if (ImGui.Selectable("Create New Cast"))
            {
                Family newFam = CreateNewFamily(in_Scene.Scene.Value);
                Cast newCast = CreateNewCastFromDefault("New_Cast", null);
                newFam.Add(newCast);
                in_Scene.Casts.Add(new SVisibilityData.SCast(newCast));
            }
        }
        public static Action SceneRightClickAction(SVisibilityData.SScene in_Scene)
        {
            Action rightClick = () =>
            {
                AddCastOption(in_Scene);
            };
            return rightClick;
        }
        private static void DrawSceneNode(SVisibilityData.SNode in_VisNode)
        {
            bool selectedNode = false;
            bool selectedScene = false;
            //Scene Node
            if (ImKunaiTreeNode.VisibilityNode($"{in_VisNode.Node.Key}", ref in_VisNode.Active, ref selectedNode, in_Icon: NodeIconResource.SceneNode))
            {

                foreach (var inNode in in_VisNode.Nodes)
                    DrawSceneNode(inNode);
                foreach (SVisibilityData.SScene g in in_VisNode.Scene)
                {
                    
                    //Scene
                    if (ImKunaiTreeNode.VisibilityNode(g.Scene.Key, ref g.Active, ref selectedScene, SceneRightClickAction(g), in_Icon: NodeIconResource.Scene))
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
        public override void Update(KunaiProject in_Proj)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, MenuBarWindow.MenuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(ImGui.GetWindowViewport().Size.X / 4.5f, ImGui.GetWindowViewport().Size.Y), ImGuiCond.Always);
            if (ImGui.Begin("Hierarchy", MainWindow.WindowFlags))
            {
                ImGui.BeginDisabled(true);
                _searchBox.Render();
                ImGui.EndDisabled();
                ImKunaiTreeNode.ItemRowsBackground(new System.Numerics.Vector4(20, 20, 20, 64));
                //if (ImGui.BeginListBox("##hierarchylist", new System.Numerics.Vector2(-1, -1)))
                //{

                    if (in_Proj.WorkProjectCsd != null)
                    {
                        foreach (var f in MainWindow.Renderer.VisibilityData.Nodes)
                        {
                            DrawSceneNode(f);
                        }

                    }
                    //ImGui.EndListBox();
                //}
            }
            ImGui.End();
        }
    }
}
