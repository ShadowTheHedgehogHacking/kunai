using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Numerics;
using System;
using Kunai.ShurikenRenderer;

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
    public class KunaiSprite
    {
        public Vector2 Start { get; set; }
        public Vector2 Dimensions { get; set; }
        public Texture Texture { get; set; }
        public Crop Crop;

        // Used for saving to avoid corruption in un-edited values
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

        public CroppedBitmap CroppedImage { get; set; }

        private void CreateCrop()
        {
            if (X + Width <= Texture.Width && Y + Height <= Texture.Height)
            {
                if (Width > 0 && Height > 0)
                    CroppedImage = new CroppedBitmap(Texture.ImageSource, new Int32Rect(X, Y, Width, Height));
            }
        }
        public Vector2[] GetImGuiUv()
        {
            Vector2 uvTl = new Vector2(
                Start.X / Texture.Width,
                -(Start.Y / Texture.Height));

            Vector2 uvBr = uvTl + new Vector2(
            Dimensions.X / Texture.Width,
            -(Dimensions.Y / Texture.Height));

            return [uvTl, uvBr];
        }

        public void Recalculate()
        {
            var textureSize = Texture.Size;
            Crop.TopLeft.X = Start.X / textureSize.X;
            Crop.TopLeft.Y = Start.Y / textureSize.Y;
            Crop.BottomRight.X = (Start.X + Dimensions.X) / textureSize.X;
            Crop.BottomRight.Y = (Start.Y + Dimensions.Y) / textureSize.Y;
        }
        public void GenerateCoordinates(Vector2 in_TextureSize)
        {
            var oLeft = Crop.TopLeft.X;
            var oRight = Crop.BottomRight.X;
            var oTop = Crop.TopLeft.Y;
            var oBtm = Crop.BottomRight.Y;
            var start1X = MathF.Round(oLeft * in_TextureSize.X);
            var start1Y = MathF.Round(oTop * in_TextureSize.Y);
            Start = new Vector2(start1X, start1Y);
            Start = new Vector2(Math.Clamp(Start.X, 0, in_TextureSize.X), Math.Clamp(Start.Y, 0, in_TextureSize.Y));
            Dimensions = new Vector2(MathF.Round((oRight - oLeft) * in_TextureSize.X), MathF.Round((oBtm - oTop) * in_TextureSize.Y));
        }
        public KunaiSprite(Texture in_Tex, float in_Top = 0.0f, float in_Left = 0.0f, float in_Bottom = 1.0f, float in_Right = 1.0f)
        {
            Texture = in_Tex;

            Crop = new Crop();
            Crop.TextureIndex = (uint)SpriteHelper.Textures.IndexOf(in_Tex);
            Crop.TopLeft = new Vector2(in_Left, in_Top);
            Crop.BottomRight = new Vector2(in_Right, in_Bottom);
            CreateCrop();

            HasChanged = false;
            GenerateCoordinates(in_Tex.Size);
        }

        public KunaiSprite()
        {
            Start = new Vector2();
            Dimensions = new Vector2();

            Texture = new Texture();
            HasChanged = false;
        }
    }
}