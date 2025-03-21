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
            public CsdVisData.Cast SelectedCast;
            public CsdVisData.Scene SelectedScene;
        }
    }
}
