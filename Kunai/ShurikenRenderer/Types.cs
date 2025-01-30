using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Numerics;
using System;

namespace Shuriken.Rendering
{
    public enum DrawType : uint
    {
        [Description("No Draw")]
        None,
        [Description("Sprite")]
        Sprite,
        [Description("Font")]
        Font
    }
    [Flags]
    public enum AnimationType : uint
    {
        None = 0,
        HideFlag = 1,
        XPosition = 2,
        YPosition = 4,
        Rotation = 8,
        XScale = 16,
        YScale = 32,
        SubImage = 64,
        Color = 128,
        GradientTl = 256,
        GradientBl = 512,
        GradientTr = 1024,
        GradientBr = 2048
    }
    [Flags]
    public enum ElementInheritanceFlags
    {
        None = 0,
        InheritRotation = 0x2,
        InheritColor = 0x8,
        InheritXPosition = 0x100,
        InheritYPosition = 0x200,
        InheritScaleX = 0x400,
        InheritScaleY = 0x800,
    }
    [Flags]
    public enum ElementMaterialFlags
    {
        None = 0,
        AdditiveBlending = 0x1,
        MirrorX = 0x400,
        MirrorY = 0x800,
        LinearFiltering = 0x1000
    }
    public enum ElementMaterialFiltering
    {
        [Description("Nearest")]
        NearestNeighbor = 0,
        [Description("Linear")]
        Linear = 1
    }
    public enum ElementMaterialBlend
    {
        [Description("Normal")]
        Normal = 0,
        [Description("Additive")]
        Additive = 1
    }
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
    public class Quad
    {
        public Vertex TopLeft;
        public Vertex TopRight;
        public Vertex BottomLeft;
        public Vertex BottomRight;
        public Texture Texture;
        public int ZIndex;
        public bool Additive;
        public bool LinearFiltering;
    }
    public class Sprite
    {
        public readonly int Id;
        public Vector2 Start { get; set; }
        public Vector2 Dimensions { get; set; }
        public Texture Texture { get; set; }

        // Used for saving to avoid corruption in un-edited values
        public float OriginalTop { get; set; }
        public float OriginalBottom { get; set; }
        public float OriginalLeft { get; set; }
        public float OriginalRight { get; set; }
        public bool HasChanged { get; set; }

        public Vector2 RelativeStart
        {
            get
            {
                return Start / Texture.Size;
            }
            set
            {
                Start = value * Texture.Size;
            }
        }
        public Vector2 RelativeSize
        {
            get
            {
                return Dimensions / Texture.Size;
            }
            set
            {
                Dimensions = value * Texture.Size;
            }
        }
        public int X
        {
            get { return (int)Start.X; }
            set { Start = new Vector2(value, Start.Y); CreateCrop(); HasChanged = true; }
        }

        public int Y
        {
            get { return (int)Start.Y; }
            set { Start = new Vector2(Start.X, value); CreateCrop(); HasChanged = true; }
        }

        public int Width
        {
            get { return (int)Dimensions.X; }
            set
            {
                if (X + value <= Texture.Width)
                {
                    Dimensions = new Vector2(value, Dimensions.Y);
                    CreateCrop();
                    HasChanged = true;
                }
            }
        }

        public int Height
        {
            get { return (int)Dimensions.Y; }
            set
            {
                if (Y + value <= Texture.Height)
                {
                    Dimensions = new Vector2(Dimensions.X, value);
                    CreateCrop();
                    HasChanged = true;
                }
            }
        }

        public CroppedBitmap Crop { get; set; }

        private void CreateCrop()
        {
            if (X + Width <= Texture.Width && Y + Height <= Texture.Height)
            {
                if (Width > 0 && Height > 0)
                    Crop = new CroppedBitmap(Texture.ImageSource, new Int32Rect(X, Y, Width, Height));
            }
        }
        public Vector2[] GetImGuiUV()
        {
            Vector2 uvTl = new Vector2(
                Start.X / Texture.Width,
                -(Start.Y / Texture.Height));

            Vector2 uvBr = uvTl + new Vector2(
            Dimensions.X / Texture.Width,
            -(Dimensions.Y / Texture.Height));

            return [uvTl, uvBr];
        }

        public void GenerateCoordinates(Vector2 in_TextureSize)
        {
            Start = new Vector2(MathF.Round(OriginalLeft * in_TextureSize.X), MathF.Round(OriginalTop * in_TextureSize.Y));
            Start = new Vector2(Math.Clamp(Start.X, 0, in_TextureSize.X), Math.Clamp(Start.Y, 0, in_TextureSize.Y));
            Dimensions = new Vector2(MathF.Round((OriginalRight - OriginalLeft) * in_TextureSize.X), MathF.Round((OriginalBottom - OriginalTop) * in_TextureSize.Y));
        }
        public Sprite(int in_Id, Texture in_Tex, float in_Top = 0.0f, float in_Left = 0.0f, float in_Bottom = 1.0f, float in_Right = 1.0f)
        {
            Id = in_Id;
            Texture = in_Tex;

            CreateCrop();

            OriginalTop = in_Top;
            OriginalLeft = in_Left;
            OriginalBottom = in_Bottom;
            OriginalRight = in_Right;
            HasChanged = false;
            GenerateCoordinates(in_Tex.Size);
        }

        public Sprite()
        {
            Start = new Vector2();
            Dimensions = new Vector2();

            Texture = new Texture();
            HasChanged = false;
        }
    }
}