using HekonrayBase;
using HekonrayBase.Base;
using Hexa.NET.ImGui;
using IconFonts;
using Kunai.ShurikenRenderer;
using SharpNeedle.Framework.Ninja.Csd;
using Shuriken.Rendering;
using System;
using System.Numerics;
using static Kunai.Window.ImKunai;

namespace Kunai.Window
{
    public class HierarchyWindow : Singleton<HierarchyWindow>, IWindow
    {
        private static TempSearchBox ms_SearchBox = new TempSearchBox();
        private static bool CastControl(SVisibilityData.SCast in_Vis, Cast in_Cast, bool in_IsLeaf)
        {
            SIconData icon = new();
            switch (in_Cast.GetDrawType())
            {
                case DrawType.Sprite:
                    {
                        icon = NodeIconResource.CastSprite;
                        break;
                    }
                case DrawType.None:
                    {
                        icon = NodeIconResource.CastNull;
                        break;
                    }
                case DrawType.Font:
                    {
                        icon = NodeIconResource.CastFont;
                        break;
                    }
            }
            bool selectedCast = false;
            bool returnVal = VisibilityNode(in_Cast.Name, ref in_Vis.Active, ref selectedCast, CastRightClickAction(in_Vis), in_ShowArrow: in_IsLeaf, in_Icon: icon, in_Id: $"##{in_Cast.Name}_{in_Vis.Id}");

            if (selectedCast)
            {
                InspectorWindow.SelectScene(in_Vis.Parent.Scene);
                InspectorWindow.SelectCast(in_Cast);
            }
            return returnVal;

        }
        private static void RecursiveCastWidget(SVisibilityData.SScene in_Scene, Cast in_Cast)
        {
            var vis = in_Scene.GetVisibility(in_Cast);
            if (vis == null)
                return;

            //Casts
            if (CastControl(vis, in_Cast, in_Cast.Children.Count > 0))
            {
                for (int x = 0; x < in_Cast.Children.Count; x++)
                {
                    RecursiveCastWidget(in_Scene, in_Cast.Children[x]);
                }
                ImGui.TreePop();
            }
        }

        private static Action SceneNodeRightClickAction(SVisibilityData.SNode in_Cast)
        {
            void RightClick()
            {
                if (ImGui.Selectable("New Scene"))
                {
                    CreationHelper.CreateNewScene(in_Cast);
                }
                //if (ImGui.Selectable("Delete"))
                //    in_Cast.Parent.Remove(in_Cast);
            }

            return RightClick;
        }

        private static Action CastRightClickAction(SVisibilityData.SCast in_Cast)
        {
            void RightClick()
            {
                if (ImGui.BeginMenu("New Cast..."))
                {
                    if (ImGui.MenuItem("Null Cast"))
                    {
                        CreationHelper.CreateNewCast(in_Cast, DrawType.None);
                    }

                    if (ImGui.MenuItem("Sprite Cast"))
                    {
                        CreationHelper.CreateNewCast(in_Cast, DrawType.Sprite);
                    }

                    if (ImGui.MenuItem("Font Cast"))
                    {
                        CreationHelper.CreateNewCast(in_Cast, DrawType.Font);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.Selectable("Delete")) in_Cast.Parent.Remove(in_Cast);
            }

            return RightClick;
        }

        private static Action SceneRightClickAction(SVisibilityData.SScene in_Scene)
        {
            void RightClick()
            {
                if (ImGui.BeginMenu("New Cast..."))
                {
                    if (ImGui.MenuItem("Null Cast"))
                    {
                        CreationHelper.CreateNewCast(in_Scene, DrawType.None);
                    }

                    if (ImGui.MenuItem("Sprite Cast"))
                    {
                        CreationHelper.CreateNewCast(in_Scene, DrawType.Sprite);
                    }

                    if (ImGui.MenuItem("Font Cast"))
                    {
                        CreationHelper.CreateNewCast(in_Scene, DrawType.Font);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.Selectable("Delete")) in_Scene.Parent.Remove(in_Scene);
            }

            return RightClick;
        }
        private static void DrawSceneNode(SVisibilityData.SNode in_VisNode)
        {
            bool selectedNode = false;
            bool selectedScene = false;
            //Scene Node
            if (VisibilityNode($"{in_VisNode.Node.Key}", ref in_VisNode.Active, ref selectedNode, SceneNodeRightClickAction(in_VisNode), in_Icon: NodeIconResource.SceneNode))
            {
                foreach (var inNode in in_VisNode.Nodes)
                {
                    DrawSceneNode(inNode);
                }
                for (int i = 0; i < in_VisNode.Scene.Count; i++)
                {
                    SVisibilityData.SScene scene = in_VisNode.Scene[i];

                    // If the user is searching, show only the casts with the searched name
                    // if the user isn't searching, show a tree view of the scene
                    // will all the casts and their own children
                    if (ms_SearchBox.IsSearching)
                    {
                        // If there's no cast in the scene with the name, skip the scene
                        foreach (var cast in scene.Casts)
                        {
                            ms_SearchBox.Update(cast.Cast.Name);
                            if (ms_SearchBox.MatchResult())
                            {
                                if (CastControl(cast, cast.Cast, false))
                                    ImGui.TreePop();
                            }
                        }

                    }
                    else
                    {
                        //Scene
                        if (VisibilityNode(scene.Scene.Key, ref scene.Active, ref selectedScene, SceneRightClickAction(scene), in_Icon: NodeIconResource.Scene))
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
                    
                }

                ImGui.TreePop();
            }
        }

        public void OnReset(IProgramProject in_Renderer)
        {
        }

        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (KunaiProject)in_Renderer;
            ImGui.SetNextWindowPos(new Vector2(0, MenuBarWindow.MenuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(
                new Vector2(ImGui.GetWindowViewport().Size.X / 4.5f,
                    ImGui.GetWindowViewport().Size.Y - MenuBarWindow.MenuBarHeight), ImGuiCond.Always);
            if (ImGui.Begin("Hierarchy", MainWindow.WindowFlags))
            {
                ms_SearchBox.Render();

                ImGui.BeginDisabled(true);
                ImGui.PushFont(ImGuiController.FontAwesomeFont);
                Vector2 size = new Vector2(24, 24);
                if (ImGui.Button(NodeIconResource.SceneNode.Icon, size))
                {

                }
                ImGui.SameLine();
                if (ImGui.Button(NodeIconResource.Scene.Icon, size))
                {

                }
                ImGui.SameLine();
                if (ImGui.Button(NodeIconResource.CastNull.Icon, size))
                {

                }
                ImGui.SameLine();
                if (ImGui.Button(FontAwesome6.TurnUp, size))
                {

                }
                ImGui.SameLine();
                if (ImGui.Button(FontAwesome6.TurnDown, size))
                {

                }
                ImGui.PopFont();
                ImGui.EndDisabled();
                ItemRowsBackground(new Vector4(20, 20, 20, 64));
                if(BeginListBoxCustom("##hierarchylist", new Vector2(-1,-1)))
                {
                    if (renderer.WorkProjectCsd != null)
                    {
                        foreach (var f in renderer.VisibilityData.Nodes)
                        {
                            DrawSceneNode(f);
                        }
                    }
                    EndListBoxCustom();
                }
            }
            ImGui.End();
        }
    }
}
