using Shuriken.Rendering;

namespace Kunai.ShurikenRenderer
{
    public struct SReferenceImageData
    {
        public bool enabled;
        public Texture texture;
        public KunaiSprite sprite;
        public float opacity;
        public SReferenceImageData()
        {
            opacity = 1;
        }
    }
}