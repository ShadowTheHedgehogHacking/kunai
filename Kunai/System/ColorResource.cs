using Hexa.NET.ImGui;
using IconFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace Kunai
{
    public static class ColorResource
    {
        public static Vector4 SceneNode { get { return new System.Numerics.Vector4(0.992156f, 0.76078f, 0, 1); } }
        public static Vector4 Scene { get { return new System.Numerics.Vector4(0, 0.75f, 0.48039f, 1); } }
        public static Vector4 CastNull { get { unsafe { return *ImGui.GetStyleColorVec4(ImGuiCol.Text); } } }
        public static Vector4 CastSprite { get { return new Vector4(0.26666f, 0.69411f, 1, 1);} }
        public static Vector4 CastFont { get { return new Vector4(1, 0.509803f, 0.15686274f, 1); } }

        //Anims
        public static Vector4 HideFlag { get { return new Vector4(1.0f, 0.388f, 0.278f, 1.0f); } }
        public static Vector4 PositionX { get { return new Vector4(0.118f, 0.565f, 1.0f, 1.0f); } }
        public static Vector4 PositionY { get { return new Vector4(0.196f, 0.804f, 0.196f, 1.0f); } }
        public static Vector4 Rotation { get { return new Vector4(1.0f, 0.647f, 0.0f, 1.0f); } }
        public static Vector4 ScaleX { get { return new Vector4(0.729f, 0.333f, 0.827f, 1.0f); } }
        public static Vector4 ScaleY { get { return new Vector4(0.275f, 0.510f, 0.706f, 1.0f); } }
        public static Vector4 SpriteIndex { get { return new Vector4(0.863f, 0.078f, 0.235f, 1.0f); } }
        public static Vector4 Color { get { return new Vector4(1.0f, 0.843f, 0.0f, 1.0f); } }
        public static Vector4 GradientTopLeft { get { return new Vector4(0.275f, 1.0f, 0.706f, 1.0f); } }
        public static Vector4 GradientBottomLeft { get { return new Vector4(1.0f, 0.412f, 0.706f, 1.0f); } }
        public static Vector4 GradientTopRight { get { return new Vector4(0.0f, 0.749f, 1.0f, 1.0f); } }
        public static Vector4 GradientBottomRight { get { return new Vector4(0.604f, 0.804f, 0.196f, 1.0f); } }

    }
    public static class NodeIconResource
    {
        private static SIconData sceneNode = new SIconData(FontAwesome6.FolderClosed, ColorResource.SceneNode);
        private static SIconData scene = new SIconData(FontAwesome6.Film, ColorResource.Scene);
        private static SIconData castNull = new SIconData(FontAwesome6.SquarePlus, ColorResource.CastNull);
        private static SIconData castSpr = new SIconData(FontAwesome6.Image, ColorResource.CastSprite);
        private static SIconData castFont = new SIconData(FontAwesome6.Font, ColorResource.CastFont);
        private static SIconData hideFlag = new SIconData(FontAwesome6.Square, "Hide Flag", ColorResource.HideFlag);
        private static SIconData positionX = new SIconData(FontAwesome6.LeftRight, "X Translation", ColorResource.PositionX);
        private static SIconData positionY = new SIconData(FontAwesome6.UpDown, "Y Translation", ColorResource.PositionY);
        private static SIconData rotation = new SIconData(FontAwesome6.ArrowsRotate, "Rotation", ColorResource.Rotation);
        private static SIconData scaleX = new SIconData(FontAwesome6.Expand, "X Scale", ColorResource.ScaleX);
        private static SIconData scaleY = new SIconData(FontAwesome6.UpRightAndDownLeftFromCenter, "Y Scale", ColorResource.ScaleY);
        private static SIconData spriteIndex = new SIconData(FontAwesome6.PhotoFilm, "Crop", ColorResource.SpriteIndex);
        private static SIconData color = new SIconData(FontAwesome6.Palette, "Color", ColorResource.Color);
        private static SIconData gradientTopLeft = new SIconData(FontAwesome6.Palette, "TL Color", ColorResource.GradientTopLeft);
        private static SIconData gradientBottomLeft = new SIconData(FontAwesome6.Palette, "BL Color", ColorResource.GradientBottomLeft);
        private static SIconData gradientTopRight = new SIconData(FontAwesome6.Palette, "TR Color", ColorResource.GradientTopRight);
        private static SIconData gradientBottomRight = new SIconData(FontAwesome6.Palette, "BR Color", ColorResource.GradientBottomRight);


        public static SIconData SceneNode => sceneNode;
        public static SIconData Scene => scene;
        public static SIconData CastNull => castNull;
        public static SIconData CastSprite => castSpr;
        public static SIconData CastFont => castFont;
        public static SIconData HideFlag => hideFlag;
        public static SIconData PositionX => positionX;
        public static SIconData PositionY => positionY;
        public static SIconData Rotation => rotation;
        public static SIconData ScaleX => scaleX;
        public static SIconData ScaleY => scaleY;
        public static SIconData SpriteIndex => spriteIndex;
        public static SIconData Color => color;
        public static SIconData GradientTopLeft => gradientTopLeft;
        public static SIconData GradientBottomLeft => gradientBottomLeft;
        public static SIconData GradientTopRight => gradientTopRight;
        public static SIconData GradientBottomRight => gradientBottomRight;
    }
}
