using Hexa.NET.ImGui;
using SharpNeedle.Framework.Ninja.Csd;
using Shuriken.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using static Kunai.Window.ImKunai;

namespace Kunai
{
    public partial class CsdVisData
    {
        public class Scene : TVisibility<KeyValuePair<string, SharpNeedle.Framework.Ninja.Csd.Scene>>
        {
            public List<Animation> Animation = new List<Animation>();
            public List<Cast> Casts = new List<Cast>();
            public Node Parent;
            public override TVisHierarchyResult DrawHierarchy()
            {
                bool selectedScene = false;
                bool open = VisibilityNode(Value.Key, ref Active, ref selectedScene, SceneRightClickAction, in_Icon: NodeIconResource.Scene);
                return new TVisHierarchyResult(open, selectedScene);
            }

            private void SceneRightClickAction()
            {
                if (ImGui.BeginMenu("New Cast..."))
                {
                    if (ImGui.MenuItem("Null Cast"))
                    {
                        CreationHelper.CreateNewCast(this, DrawType.None);
                    }

                    if (ImGui.MenuItem("Sprite Cast"))
                    {
                        CreationHelper.CreateNewCast(this, DrawType.Sprite);
                    }

                    if (ImGui.MenuItem("Font Cast"))
                    {
                        CreationHelper.CreateNewCast(this, DrawType.Font);
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
