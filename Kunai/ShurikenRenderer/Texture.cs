using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using DirectXTexNet;
using Shuriken.Rendering.Gvr;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Interop;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Shuriken.Rendering
{    
    public class Texture
    {
        public string Name { get; }
        public string FullName { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector2 Size { get { return new Vector2(Width, Height); } set { Width = (int)value.X; Height = (int)value.Y; } }

        public BitmapSource ImageSource { get; private set; }
        internal GlTexture GlTex { get; private set; }
        public List<int> Sprites { get; set; }

        private void CreateTexture(ScratchImage in_Img)
        {
            if (TexHelper.Instance.IsCompressed(in_Img.GetMetadata().Format))
                in_Img = in_Img.Decompress(DXGI_FORMAT.B8G8R8A8_UNORM);

            else if (in_Img.GetMetadata().Format != DXGI_FORMAT.B8G8R8A8_UNORM)
                in_Img = in_Img.Convert(DXGI_FORMAT.B8G8R8A8_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0.5f);

            Width = in_Img.GetImage(0).Width;
            Height = in_Img.GetImage(0).Height;

            GlTex = new GlTexture(in_Img.FlipRotate(TEX_FR_FLAGS.FLIP_VERTICAL).GetImage(0).Pixels, Width, Height);

            CreateBitmap(in_Img);

            in_Img.Dispose();
        }

        public void Destroy()
        {
            if(GlTex != null)
            GL.DeleteTexture(GlTex.Id);
        }
        /// <summary>
        /// Used for GVR textures for GNCPs, converts GVR's to BitmapSource and output a pixel array for the GL WPF Control
        /// </summary>
        /// <param name="in_Gvr">GVR Texture</param>
        /// <param name="out_Pixels">Pixel array output for GL</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the GVR pixel array is null</exception>
        public static BitmapSource LoadTga(GvrFile in_Gvr, ref byte[] in_OutPixels)
        {
            if (in_Gvr.Pixels == null) throw new ArgumentNullException("GVR Image might be invalid, pixel array is null.");
            var pixelFormat = PixelFormats.Bgr32; //temporary!!!!!!

            ///FIX PLEASE THIS IS HORRIBLE AND MAKES THIS NOT FULLY FUNCTIONAL
            if (in_Gvr.DataFormat == GvrDataFormat.Index4 || in_Gvr.DataFormat == GvrDataFormat.Index8)
            {
                var bitmap2 = new WriteableBitmap(
                in_Gvr.Width, in_Gvr.Height,
                96, 96,
                pixelFormat,
                null
                );
                return bitmap2;
            }
            int bytesPerPixel = pixelFormat.BitsPerPixel / 8;
            int stride = in_Gvr.Width * bytesPerPixel;

            var bitmap = new WriteableBitmap(
                in_Gvr.Width, in_Gvr.Height,
                96, 96,
                pixelFormat,
                null
            );

            bitmap.WritePixels(
                new Int32Rect(0, 0, in_Gvr.Width, in_Gvr.Height),
                in_Gvr.Pixels,
                stride,
                0
            );
            //Flip vertically
            TransformedBitmap transformedBitmap = new TransformedBitmap();
            WriteableBitmap bmpClone = bitmap.Clone();
            transformedBitmap.BeginInit();
            transformedBitmap.Source = bmpClone;
            ScaleTransform transform = new ScaleTransform(1, -1, 0, 0);
            transformedBitmap.Transform = transform;
            transformedBitmap.EndInit();

            in_OutPixels = new byte[stride * transformedBitmap.PixelHeight];

            transformedBitmap.CopyPixels(in_OutPixels, stride, 0);

            return bitmap;
        }
        private unsafe void CreateTextureGvr(GvrFile in_Gvr)
        {
            Width = in_Gvr.Width;
            Height = in_Gvr.Height;

            byte[] forGlTex = null;
            var bmp = LoadTga(in_Gvr, ref forGlTex);
            if (bmp == null)
                return;

            fixed (byte* pBytes = forGlTex)
                GlTex = new GlTexture((nint)pBytes, Width, Height);
            ImageSource = bmp;
        }

        private unsafe void CreateTexture(byte[] in_Bytes)
        {
            fixed (byte* pBytes = in_Bytes)
                CreateTexture(TexHelper.Instance.LoadFromDDSMemory((nint)pBytes, in_Bytes.Length, DDS_FLAGS.NONE));
        }

        private void CreateTexture(string in_Filename)
        {
            CreateTexture(TexHelper.Instance.LoadFromDDSFile(in_Filename, DDS_FLAGS.NONE));
        }

        private void CreateBitmap(ScratchImage in_Img)
        {
            var bmp = BitmapConverter.FromTextureImage(in_Img, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ImageSource = BitmapConverter.FromBitmap(bmp);

            in_Img.Dispose();
            bmp.Dispose();
        }

        public Texture(string in_Filename, bool in_GvrTex = false) : this()
        {
            FullName = in_Filename;
            if(string.IsNullOrEmpty(in_Filename))
            {
                return;
            }
            Name = Path.GetFileNameWithoutExtension(in_Filename);
            if (in_GvrTex)
            {
                GvrFile gVr = new GvrFile();
                gVr.LoadFromGvrFile(in_Filename.ToLower());
                CreateTextureGvr(gVr);
            }
            else
                CreateTexture(in_Filename);
        }

        public Texture(string in_Name, byte[] in_Bytes) : this()
        {
            FullName = in_Name;
            Name = in_Name;
            CreateTexture(in_Bytes);
        }

        public Texture()
        {
            Name = FullName = "";
            Width = Height = 0;
            ImageSource = null;
            GlTex = null;

            Sprites = new List<int>();
        }
    }
}
