using System.IO;

namespace Shuriken.Rendering.Gvr
{
    public abstract class ImageDataFormatBase
    {
        public ushort Width { get; set; }
        public ushort Height { get; set; }

        public abstract uint BitsPerPixel { get; }

        public abstract uint DecodedDataLength { get; }
        public abstract uint EncodedDataLength { get; }

        public abstract byte TgaAlphaChannelBits { get; }

        public ImageDataFormatBase(ushort in_Width, ushort in_Height)
        {
            Width = in_Width;
            Height = in_Height;
        }

        public byte[] Decode(Stream in_InputStream)
        {
            byte[] input = new byte[EncodedDataLength];
            in_InputStream.Read(input, 0, input.Length);

            return Decode(input);
        }

        public byte[] Encode(Stream in_InputStream)
        {
            byte[] input = new byte[DecodedDataLength];
            in_InputStream.Read(input, 0, input.Length);

            return Encode(input);
        }

        public abstract byte[] Decode(byte[] in_Input);
        public abstract byte[] Encode(byte[] in_Input);
    }
}