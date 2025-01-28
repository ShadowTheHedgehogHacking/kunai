namespace Shuriken.Models
{
    public class CastTransform
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }
        public Vector4 Color { get; set; }

        public CastTransform()
        {
            Position = new Vector2();
            Rotation = 0;
            Scale = new Vector2(1, 1);
            Color = new Vector4(1, 1, 1, 1);
        }

        public CastTransform(Vector2 in_Position, float in_Rotation, Vector2 in_Scale, Vector4 in_Color)
        {
            Position = in_Position;
            Rotation = in_Rotation;
            Scale = in_Scale;
            Color = in_Color;
        }
    }
}