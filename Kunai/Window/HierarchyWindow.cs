using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using SharpNeedle.Ninja.Csd;
using Shuriken.Rendering;
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
            if (ImKunaiTreeNode.VisibilityNode(in_Cast.Name, ref vis.Active, ref selectedcast, in_ShowArrow: in_Cast.Children.Count > 0, in_Icon: icon, in_ID: $"##{in_Cast.Name}_{vis.ID}"))
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
        public static Cast CreateNewCastFromDefault(string in_Name, Cast in_Parent, DrawType in_Type)
        {
            Cast newCast = new Cast();
            newCast.SpriteIndices = new int[32];
            for (int i = 0; i < 32; i++)
                newCast.SpriteIndices[i] = -1;
            newCast.Parent = in_Parent;
            var info = newCast.Info;
            newCast.Field04 = (uint)in_Type;
            newCast.Name = in_Name;
            info.Scale = new System.Numerics.Vector2(1, 1);
            info.SpriteIndex = -1;
            newCast.Enabled = true;
            newCast.TopLeft = new System.Numerics.Vector2(-25, -25) / KunaiProject.Instance.ViewportSize;
            newCast.BottomLeft = new System.Numerics.Vector2(-25, 25) / KunaiProject.Instance.ViewportSize;
            newCast.TopRight = new System.Numerics.Vector2(25, -25) / KunaiProject.Instance.ViewportSize;
            newCast.BottomRight = new System.Numerics.Vector2(25, 25) / KunaiProject.Instance.ViewportSize;
            info.Color = Vector4.One.ToSharpNeedleColor();
            info.GradientTopLeft = Vector4.One.ToSharpNeedleColor();
            info.GradientBottomLeft = Vector4.One.ToSharpNeedleColor();
            info.GradientTopRight = Vector4.One.ToSharpNeedleColor();
            info.GradientBottomRight = Vector4.One.ToSharpNeedleColor();
            newCast.Info = info;
            return newCast;
        }
        static void NewCast(SVisibilityData.SScene in_Scene, DrawType in_Type)
        {
            Family newFam = CreateNewFamily(in_Scene.Scene.Value);
            Cast newCast = CreateNewCastFromDefault($"Cast_{in_Scene.Casts.Count}", null, in_Type);
            newFam.Add(newCast);
            in_Scene.Casts.Add(new SVisibilityData.SCast(newCast, in_Scene));
        }
        static void AddCastOption(SVisibilityData.SScene in_Scene)
        {
            if(ImGui.BeginMenu("New Cast..."))
            {
                if (ImGui.MenuItem("Null Cast"))
                {
                    NewCast(in_Scene, DrawType.None);
                }
                if (ImGui.MenuItem("Sprite Cast"))
                {
                    NewCast(in_Scene, DrawType.Sprite);
                }
                if (ImGui.MenuItem("Font Cast"))
                {
                    NewCast(in_Scene, DrawType.Font);
                }
                ImGui.EndMenu();
            }
            if (ImGui.Selectable("Delete"))
                in_Scene.Parent.Remove(in_Scene);
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
                for (int i = 0; i < in_VisNode.Scene.Count; i++)
                {
                    SVisibilityData.SScene scene = in_VisNode.Scene[i];
                    //Scene
                    if (ImKunaiTreeNode.VisibilityNode(scene.Scene.Key, ref scene.Active, ref selectedScene, SceneRightClickAction(scene), in_Icon: NodeIconResource.Scene))
                    {
                        for (int x = 0; x < scene.Scene.Value.Families.Count; x++)
                        {
                            var family = scene.Scene.Value.Families[x];

                            var castFamilyRoot = family.Casts[0];
                            RecursiveCastWidget(scene, castFamilyRoot);

                        }
                        ImGui.TreePop();
                    }
                    if (selectedScene)
                    {
                        InspectorWindow.SelectScene(scene.Scene);
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
