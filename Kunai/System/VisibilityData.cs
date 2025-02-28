using SharpNeedle.Framework.Ninja.Csd;
using SharpNeedle.Framework.Ninja.Csd.Motions;
using Shuriken.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kunai
{


    public class SVisibilityData
    {
        public class SCast
        {
            public Cast Cast;
            public bool Active = true;
            public int ID;
            public SScene Parent;
            public SCast(Cast in_Scene, SScene parent)
            {
                ID = new Random().Next(0, 1000);
                Cast = in_Scene;
                Parent = parent;
            }
        }
        public class SAnimation
        {
            public KeyValuePair<string, Motion> Motion;
            public bool Active = true;
            public SAnimation(KeyValuePair<string, Motion> in_Scene)
            {
                Motion = in_Scene;
            }
            public KeyFrameList GetTrack(Cast in_Layer, AnimationType in_Type)
            {
                foreach (var animation in Motion.Value.FamilyMotions)
                {
                    foreach (var animtrack in animation.CastMotions)
                    {
                        if (animtrack.Cast == in_Layer && animtrack.Capacity != 0)
                        {
                            var track = animtrack.FirstOrDefault(in_T => in_T.Property.ToShurikenAnimationType() == in_Type);
                            if (track != null)
                                return track;
                        }
                    }
                }
                return null;
            }
        }
        public class SScene
        {
            public List<SAnimation> Animation = new List<SAnimation>();
            public List<SCast> Casts = new List<SCast>();
            public KeyValuePair<string, Scene> Scene;
            public SNode Parent;
            public bool Active = true;
            public SScene(KeyValuePair<string, Scene> in_Scene, SNode in_Node)
            {
                Scene = in_Scene;
                Parent = in_Node;
                foreach (var group in Scene.Value.Families)
                {
                    foreach (var cast in group.Casts)
                    {
                        Casts.Add(new SCast(cast, this));
                    }
                }
                foreach (var mot in Scene.Value.Motions)
                {
                    Animation.Add(new SAnimation(mot));
                }
            }
            public SCast GetVisibility(Cast in_Cast)
            {
                return Casts.FirstOrDefault(in_Node => in_Node.Cast == in_Cast);
            }

            public void Rename(string in_NewName)
            {
                var node = Parent.Node.Value;
                var oldIndex = node.Scenes.Find(Scene.Key);
                node.Scenes.RemoveAt(oldIndex);
                Scene = new KeyValuePair<string, Scene>(in_NewName, Scene.Value);
                node.Scenes.Insert(Scene.Key, Scene.Value, oldIndex);
            }
            public void Remove(SCast in_Cast)
            {
                Family familyOfCast = null;
                foreach(var f in Scene.Value.Families)
                {
                    if(f.Casts.Contains(in_Cast.Cast))
                    {
                        familyOfCast = f;
                        f.Remove(in_Cast.Cast);
                        break;
                    }
                }
                if(familyOfCast != null)
                    if(familyOfCast.Count == 0)
                        Scene.Value.Families.Remove(familyOfCast);
                Casts.Remove(in_Cast);
            }
        }
        public class SNode
        {

            public List<SScene> Scene = new List<SScene>();
            public List<SNode> Nodes = new List<SNode>();
            public KeyValuePair<string, SceneNode> Node;
            public bool Active = true;

            public SNode(KeyValuePair<string, SceneNode> in_Node)
            {
                Node = in_Node;
                foreach (var scene in Node.Value.Scenes)
                {
                    Scene.Add(new SScene(scene, this));
                }
                foreach (var scene in in_Node.Value.Children)
                {
                    Nodes.Add(new SNode(scene));
                }
            }
            public SScene GetVisibility(Scene in_Scene)
            {
                return Scene.FirstOrDefault(in_Node => in_Node.Scene.Value == in_Scene);
            }

            internal void Remove(SScene in_Scene)
            {
                in_Scene.Animation.Clear();
                Node.Value.Scenes.Remove(in_Scene.Scene);
                Scene.Remove(in_Scene);
            }
        }

        public List<SNode> Nodes = new List<SNode>();

        public SVisibilityData(CsdProject in_Proj)
        {
            Nodes.Add(new SNode(new KeyValuePair<string, SceneNode>("Root", in_Proj.Project.Root)));

        }
        private SNode RecursiveGetNode(SNode in_Node, SceneNode in_SceneNode)
        {
            if (in_Node.Node.Value == in_SceneNode)
                return in_Node;
            foreach (var node in in_Node.Nodes)
            {
                if (node.Node.Value == in_SceneNode)
                    return node;
            }
            return null;
        }
        private SScene RecursiveGetScene(SNode in_Node, Scene in_Scene)
        {
            foreach (var s in in_Node.Scene)
            {
                if (s.Scene.Value == in_Scene)
                    return s;
            }
            foreach (var node in in_Node.Nodes)
            {
                var scene = RecursiveGetScene(node, in_Scene);
                if (scene != null)
                    return scene;
            }
            return null;
        }
        public SNode GetVisibility(SceneNode in_Scene)
        {
            foreach (var node in Nodes)
            {
                return RecursiveGetNode(node, in_Scene);
            }
            return null;
        }
        public SScene GetScene(Scene in_Scene)
        {
            foreach (var node in Nodes)
            {
                return RecursiveGetScene(node, in_Scene);
            }
            return null;
        }
    }
}
