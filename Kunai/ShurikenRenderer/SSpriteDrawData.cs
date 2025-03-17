using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;
using SharpNeedle.Framework.Ninja.Csd;



namespace Shuriken.Rendering
{
    public struct SSpriteDrawData
    {
        public int ZIndex;
        public bool Hidden;
        public bool OverrideUvCoords;
        public Vector2 TopLeft, BottomLeft, TopRight, BottomRight;
        public Vector2 Position;
        public float Rotation;
        public Vector2 Scale;
        public float AspectRatio;
        public KunaiSprite Sprite; 
        public KunaiSprite NextSprite;
        public float SpriteFactor;
        public Vector4 Color;
        public Vector4 GradientTopLeft;
        public Vector4 GradientBottomLeft;
        public Vector4 GradientTopRight;
        public Vector4 GradientBottomRight;
        public ElementMaterialFlags Flags;
        public Cast OriginCast;

        public SSpriteDrawData(Cast in_Cast, Scene in_Scene)
        {
            Hidden = in_Cast.Info.HideFlag != 0;
            Position = new Vector2(in_Cast.Info.Translation.X, in_Cast.Info.Translation.Y);
            Rotation = in_Cast.Info.Rotation;
            Scale = new Vector2(in_Cast.Info.Scale.X, in_Cast.Info.Scale.Y);
            Color = in_Cast.Info.Color.ToVec4();
            GradientTopLeft = in_Cast.Info.GradientTopLeft.ToVec4();
            GradientBottomLeft = in_Cast.Info.GradientBottomLeft.ToVec4();
            GradientTopRight = in_Cast.Info.GradientTopRight.ToVec4();
            GradientBottomRight = in_Cast.Info.GradientBottomRight.ToVec4();
            ZIndex = (int)in_Scene.Priority + in_Cast.Priority;
            OriginCast = in_Cast;
            AspectRatio = in_Scene.AspectRatio;
            Flags = (ElementMaterialFlags)in_Cast.Field38;
        }
    }
}
