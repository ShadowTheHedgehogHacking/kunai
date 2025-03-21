using SharpNeedle.Framework.Ninja.Csd;
using SharpNeedle.Framework.Ninja.Csd.Motions;
using Shuriken.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Kunai
{
    public partial class CsdVisData
    {
        public class Animation : TVisibility<KeyValuePair<string, Motion>>
        {
            public Animation(KeyValuePair<string, Motion> in_Scene)
            {
                Value = in_Scene;
            }
            public KeyFrameList GetTrack(SharpNeedle.Framework.Ninja.Csd.Cast in_Layer, AnimationType in_Type)
            {
                foreach (var animation in Value.Value.FamilyMotions)
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
    }
}
