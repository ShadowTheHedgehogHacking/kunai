using OpenTK.Graphics.OpenGL;
namespace Shuriken.Rendering
{
    internal class GlTexture
    {
        private int _id = 0;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public GlTexture()
        {

        }

        public GlTexture(nint in_Pixels, int in_Width, int in_Height)
        {
            GL.GenTextures(1, out _id);

            GL.BindTexture(TextureTarget.Texture2D, Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, in_Width, in_Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, in_Pixels);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, _id);
        }

        public void Dispose()
        {
            GL.DeleteTextures(1, ref _id);
        }
    }

}