using SharpNeedle.Framework.Ninja.Csd;
using SharpNeedle.Framework.Ninja.Csd.Motions;
using System.Collections.Generic;

namespace Kunai.ShurikenRenderer
{

    public partial class KunaiProject
    {
        public struct SSelectionData
        {
            public KeyFrameList TrackAnimation;
            public KeyFrame KeyframeSelected;
            public Cast SelectedCast;
            public KeyValuePair<string, Scene> SelectedScene;
        }
    }
}
