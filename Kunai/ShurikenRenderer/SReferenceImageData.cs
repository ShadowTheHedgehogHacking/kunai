using Shuriken.Rendering;

namespace Kunai.ShurikenRenderer
{
    public struct SReferenceImageData
    {
        public bool Enabled;
        public Texture Texture;
        public KunaiSprite Sprite;
        public float Opacity;
        public SReferenceImageData()
        {
            Opacity = 1;
        }
    }
}