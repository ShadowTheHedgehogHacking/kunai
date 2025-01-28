using System;

namespace Shuriken.Rendering.Gvr
{
    internal class GvrImageDataFormatIndex8 : GvrImageDataFormat
    {
        public override uint BitsPerPixel => 8;

        public override uint DecodedDataLength => (uint)(Width * Height);
        public override uint EncodedDataLength => (uint)(Width * Height);

        public override byte TgaAlphaChannelBits => 0;

        public GvrImageDataFormatIndex8(ushort in_Width, ushort in_Height) : base(in_Width, in_Height)
        {

        }

        public override byte[] Decode(byte[] in_Input)
        {
            byte[] output = new byte[DecodedDataLength];
            int offset = 0;

            for (int y = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 8)
                {
                    for (int y2 = 0; y2 < 4; y2++)
                    {
                        Array.Copy(in_Input, offset, output, ((y + y2) * Width) + x, 8);
                        offset += 8;
                    }
                }
            }

            return output;
        }

        public override byte[] Encode(byte[] in_Input)
        {
            byte[] output = new byte[EncodedDataLength];
            int offset = 0;

            for (int y = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 8)
                {
                    for (int y2 = 0; y2 < 4; y2++)
                    {
                        Array.Copy(in_Input, ((y + y2) * Width) + x, output, offset, 8);
                        offset += 8;
                    }
                }
            }

            return output;
        }
    }
}