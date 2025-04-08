using Hexa.NET.ImGui;
using SharpNeedle.Framework.Ninja.Csd;
using SharpNeedle.Framework.Ninja.Csd.Motions;
using Shuriken.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Kunai
{
    public partial class CsdVisData
    {
        public class Scene : TVisibility<KeyValuePair<string, SharpNeedle.Framework.Ninja.Csd.Scene>>
        {
            public List<Animation> Animation = new List<Animation>();
            public List<Cast> Casts = new List<Cast>();
            public Node Parent;
            public Scene() { }

            public void ApplyAnimsToScene()
            {
                foreach (var anim2 in Animation)
                {
                    Value.Value.Motions.TryGetValue(anim2.Value.Key, out Motion motion);
                    if(motion != null)
                    {
                        Value.Value.Motions[anim2.Value.Key] = anim2.Value.Value;
                    }
                }
            }
            public override TVisHierarchyResult DrawHierarchy()
            {
                bool selectedScene = false;
                bool open = ImKunai.VisibilityNode(Value.Key, ref Active, ref selectedScene, SceneRightClickAction, in_Icon: NodeIconResource.Scene);
                return new TVisHierarchyResult(open, selectedScene);
            }
            public override void DrawInspector()
            {
                ImGui.SeparatorText("Scene");
                string name = Value.Key;
                int vers = Value.Value.Version;
                int priority = (int)Value.Value.Priority;
                float aspectRatio = Value.Value.AspectRatio;
                float fps = Value.Value.FrameRate;

                //Show aspect ratio as (width:height) by using greatest common divisor
                int width = 1280;
                int height = (int)(1280 / aspectRatio);
                double decimalRatio = (double)width / height;
                int gcdValue = KunaiMath.Gcd(width, height);
                int simplifiedWidth = width / gcdValue;
                int simplifiedHeight = height / gcdValue;

                if (ImGui.InputText("Name", ref name, 256))
                {
                    Rename(name);
                }
                ImGui.InputFloat("Framerate", ref fps);
                ImGui.InputFloat("Aspect Ratio", ref aspectRatio);
                ImGui.InputInt("Version", ref vers);
                ImGui.InputInt("Priority", ref priority);
                ImGui.Text($"Ratio: ({simplifiedWidth}:{simplifiedHeight})");
                ImGui.SeparatorText("Info");
                var space = ImGui.GetContentRegionAvail();
                if (ImGui.BeginListBox("##crops", new Vector2(-1, space.Y / 2 - 5)))
                {
                    int idx = 0;
                    foreach (var s in Value.Value.Sprites)
                    {
                        if (ImGui.TreeNode($"Crop ({idx})"))
                        {
                            ImGui.Text($"Texture Index: {s.TextureIndex.ToString()}");
                            ImGui.Text($"Top-Left: {s.TopLeft.ToString()}");
                            ImGui.Text($"Bottom-Right: {s.BottomRight.ToString()}");
                            ImGui.TreePop();
                        }
                        idx++;
                    }
                    ImGui.EndListBox();
                }
                ImGui.Separator();
                if (ImGui.BeginListBox("##textures", new Vector2(-1, space.Y / 2 - 5)))
                {
                    int idx = 0;
                    foreach (var s in Value.Value.Textures)
                    {
                        if (ImGui.TreeNode($"Texture ({idx})"))
                        {
                            ImGui.Text($"Size: {s.ToString()}");
                            ImGui.TreePop();
                        }
                        idx++;
                    }
                    ImGui.EndListBox();
                }
                Value.Value.AspectRatio = aspectRatio;
                Value.Value.AspectRatio = aspectRatio;
                Value.Value.FrameRate = fps;
            }
            private void SceneRightClickAction()
            {
                if (ImGui.BeginMenu("New Cast..."))
                {
                    if (ImGui.MenuItem("Null Cast"))
                    {
                        CreationHelper.CreateNewCast(this, SharpNeedle.Framework.Ninja.Csd.Cast.EType.Null);
                    }

                    if (ImGui.MenuItem("Sprite Cast"))
                    {
                        CreationHelper.CreateNewCast(this, SharpNeedle.Framework.Ninja.Csd.Cast.EType.Sprite);
                    }

                    if (ImGui.MenuItem("Font Cast"))
                    {
                        CreationHelper.CreateNewCast(this, SharpNeedle.Framework.Ninja.Csd.Cast.EType.Font);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.Selectable("Delete")) Parent.Remove(this);
            }

            public Scene(KeyValuePair<string, SharpNeedle.Framework.Ninja.Csd.Scene> in_Scene, Node in_Node)
            {
                Value = in_Scene;
                Parent = in_Node;
                foreach (var group in Value.Value.Families)
                {
                    foreach (var cast in group.Casts)
                    {
                        Casts.Add(new Cast(cast, this));
                    }
                }
                foreach (var mot in Value.Value.Motions)
                {
                    Animation.Add(new Animation(mot));
                }
            }
            public Cast GetVisibility(SharpNeedle.Framework.Ninja.Csd.Cast in_Cast)
            {
                foreach (var node in Casts)
                {
                    if (node.Value == in_Cast) return node;
                }
                return null;
            }
            public bool FindCastByName(string in_CastName)
            {
                if(string.IsNullOrEmpty(in_CastName)) return true;
                return Casts.Any(in_Cast => in_Cast.Value.Name.ToLower().Contains(in_CastName));
            }
            public void Rename(string in_NewName)
            {
                var node = Parent.Value.Value;
                var oldIndex = node.Scenes.Find(Value.Key);
                node.Scenes.RemoveAt(oldIndex);
                Value = new KeyValuePair<string, SharpNeedle.Framework.Ninja.Csd.Scene>(in_NewName, Value.Value);
                node.Scenes.Insert(Value.Key, Value.Value, oldIndex);
            }
            public void Remove(Cast in_Cast)
            {
                Family familyOfCast = null;
                foreach(var f in Value.Value.Families)
                {
                    if(f.Casts.Contains(in_Cast.Value))
                    {
                        familyOfCast = f;
                        f.Remove(in_Cast.Value);
                        break;
                    }
                }
                if(familyOfCast != null)
                    if(familyOfCast.Count == 0)
                        Value.Value.Families.Remove(familyOfCast);
                Casts.Remove(in_Cast);
            }
        }
    }
}
