namespace Shuriken.Rendering
{
    public struct Quad
    {
        public Vertex TopLeft;
        public Vertex TopRight;
        public Vertex BottomLeft;
        public Vertex BottomRight;
        public Texture Texture;
        public int ZIndex;
        public bool Additive;
        public bool LinearFiltering;
        public SSpriteDrawData OriginalData;
    }
}