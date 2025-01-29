using SharpNeedle.Ninja.Csd;
using SharpNeedle.Ninja.Csd.Motions;
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
            public SCast(Cast in_Scene)
            {
                Cast = in_Scene;
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
            public bool Active = true;
            public SScene(KeyValuePair<string, Scene> in_Scene)
            {
                Scene = in_Scene;
                foreach (var group in Scene.Value.Families)
                {
                    foreach (var cast in group.Casts)
                    {
                        Casts.Add(new SCast(cast));
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
                    Scene.Add(new SScene(scene));
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
