namespace Shuriken.Rendering.Gvr
{
    internal class GvrImageDataFormatArgb8888 : GvrImageDataFormat
    {
        public override uint BitsPerPixel => 32;

        public override uint DecodedDataLength => (uint)((Width * Height) << 2);
        public override uint EncodedDataLength => (uint)((Width * Height) << 2);

        public override byte TgaAlphaChannelBits => 8;

        public GvrImageDataFormatArgb8888(ushort in_Width, ushort in_Height) : base(in_Width, in_Height)
        {

        }

        public override byte[] Decode(byte[] in_Input)
        {
            byte[] output = new byte[DecodedDataLength];
            int offset = 0;

            for (int y = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 4)
                {
                    for (int y2 = 0; y2 < 4; y2++)
                    {
                        for (int x2 = 0; x2 < 4; x2++)
                        {
                            output[((((y + y2) * Width) + (x + x2)) * 4) + 3] = in_Input[offset + 0];
                            output[((((y + y2) * Width) + (x + x2)) * 4) + 2] = in_Input[offset + 1];
                            output[((((y + y2) * Width) + (x + x2)) * 4) + 1] = in_Input[offset + 32];
                            output[((((y + y2) * Width) + (x + x2)) * 4) + 0] = in_Input[offset + 33];

                            offset += 2;
                        }
                    }

                    offset += 32;
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
                for (int x = 0; x < Width; x += 4)
                {
                    for (int y2 = 0; y2 < 4; y2++)
                    {
                        for (int x2 = 0; x2 < 4; x2++)
                        {
                            output[offset + 00] = in_Input[((((y + y2) * Width) + (x + x2)) * 4) + 3];
                            output[offset + 01] = in_Input[((((y + y2) * Width) + (x + x2)) * 4) + 2];
                            output[offset + 32] = in_Input[((((y + y2) * Width) + (x + x2)) * 4) + 1];
                            output[offset + 33] = in_Input[((((y + y2) * Width) + (x + x2)) * 4) + 0];

                            offset += 2;
                        }
                    }

                    offset += 32;
                }
            }

            return output;
        }
    }
}