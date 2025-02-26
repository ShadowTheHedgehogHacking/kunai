using IconFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Kunai.Window.ImKunaiControls;

namespace Kunai
{
    public static class ColorResource
    {
        public static Vector4 SceneNode { get { return new System.Numerics.Vector4(0.992156f, 0.76078f, 0, 1); } }
        public static Vector4 Scene { get { return new System.Numerics.Vector4(0, 0.75f, 0.48039f, 1); } }
        public static Vector4 CastNull { get { return new Vector4(1, 1, 1, 1); } }
        public static Vector4 CastSprite { get { return new Vector4(0.26666f, 0.69411f, 1, 1);} }
        public static Vector4 CastFont { get { return new Vector4(1, 0.509803f, 0.15686274f, 1); } }
    }
    public static class NodeIconResource
    {
        public static SIconData SceneNode { get { return new SIconData(FontAwesome6.FolderClosed, ColorResource.SceneNode); } }
        public static SIconData Scene { get { return new SIconData(FontAwesome6.Film, ColorResource.Scene); } }
        public static SIconData CastNull { get { return new SIconData(FontAwesome6.SquarePlus, ColorResource.CastNull); } }
        public static SIconData CastSprite { get { return new SIconData(FontAwesome6.Image, ColorResource.CastSprite); } }
        public static SIconData CastFont { get { return new SIconData(FontAwesome6.Font, ColorResource.CastFont); } }
    }
}
