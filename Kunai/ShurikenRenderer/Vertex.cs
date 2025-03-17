using System.Numerics;

namespace Shuriken.Rendering
{
    public struct Vertex
    {
        public Vector2 Position;
        public Vector2 Uv;
        public Vector4 Color;
        public Vertex WithInvertedColor()
        {
            Color = Color.Invert();
            return this;
        }
    }
}