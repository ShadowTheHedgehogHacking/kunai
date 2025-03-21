using Hexa.NET.ImGui;
using Kunai.Window;
using SharpNeedle.Framework.Ninja.Csd;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kunai
{
    public partial class CsdVisData
    {
        public class Node : TVisibility<KeyValuePair<string, SceneNode>, Node>
        {
            public List<Scene> Scene = new List<Scene>();

            public override TVisHierarchyResult DrawHierarchy()
            {
                bool selectedNode = false;
                var result = ImKunai.VisibilityNode($"{Value.Key}", ref Active, ref selectedNode, SceneNodeRightClickAction, in_Icon: NodeIconResource.SceneNode);
                return new TVisHierarchyResult(result, selectedNode);
            }

            private void SceneNodeRightClickAction()
            {
                if (ImGui.Selectable("New Scene"))
                {
                    CreationHelper.CreateNewScene(this);
                }
            }

            public Node(KeyValuePair<string, SceneNode> in_Node)
            {
                Value = in_Node;
                foreach (var scene in Value.Value.Scenes)
                {
                    Scene.Add(new Scene(scene, this));
                }
                foreach (var scene in in_Node.Value.Children)
                {
                    Children.Add(new Node(scene));
                }
            }
            public Scene GetVisibility(SharpNeedle.Framework.Ninja.Csd.Scene in_Scene)
            {
                return Scene.FirstOrDefault(in_Node => in_Node.Value.Value == in_Scene);
            }

            internal void Remove(Scene in_Scene)
            {
                in_Scene.Animation.Clear();
                Value.Value.Scenes.Remove(in_Scene.Value);
                Scene.Remove(in_Scene);
            }
        }
    }
}
