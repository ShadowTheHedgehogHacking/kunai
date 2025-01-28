using System;

namespace Shuriken.Rendering.Gvr
{
    internal class GvrImageDataFormatDxt1 : GvrImageDataFormat
    {
        public override uint BitsPerPixel => 4;

        public override uint DecodedDataLength => (uint)(Width * Height * 4);
        public override uint EncodedDataLength => (uint)(Width * Height / 2);

        public override byte TgaAlphaChannelBits => 8;

        public GvrImageDataFormatDxt1(ushort in_Width, ushort in_Height) : base(in_Width, in_Height)
        {

        }

        public override byte[] Decode(byte[] in_Input)
        {
            byte[] output = new byte[DecodedDataLength];
            int offset = 0;

            // Palette for each 4x4 block
            byte[][] palette = new byte[4][];
            palette[0] = new byte[4];
            palette[1] = new byte[4];
            palette[2] = new byte[4];
            palette[3] = new byte[4];

            // The two colors that determine the palette
            ushort[] pixel = new ushort[2];

            for (int y = 0; y < Height; y += 8)
            {
                for (int x = 0; x < Width; x += 8)
                {
                    for (int y2 = 0; y2 < 8; y2 += 4)
                    {
                        for (int x2 = 0; x2 < 8; x2 += 4)
                        {
                            // Get the first two colors
                            pixel[0] = (ushort)((in_Input[offset + 0] << 8) | in_Input[offset + 1]);
                            pixel[1] = (ushort)((in_Input[offset + 2] << 8) | in_Input[offset + 3]);

                            palette[0][3] = 0xFF;
                            palette[0][2] = (byte)(((pixel[0] >> 11) & 0x1F) * 0xFF / 0x1F);
                            palette[0][1] = (byte)(((pixel[0] >> 5) & 0x3F) * 0xFF / 0x3F);
                            palette[0][0] = (byte)(((pixel[0] >> 0) & 0x1F) * 0xFF / 0x1F);

                            palette[1][3] = 0xFF;
                            palette[1][2] = (byte)(((pixel[1] >> 11) & 0x1F) * 0xFF / 0x1F);
                            palette[1][1] = (byte)(((pixel[1] >> 5) & 0x3F) * 0xFF / 0x3F);
                            palette[1][0] = (byte)(((pixel[1] >> 0) & 0x1F) * 0xFF / 0x1F);

                            // Determine the next two colors based on how the first two are stored
                            if (pixel[0] > pixel[1])
                            {
                                palette[2][3] = 0xFF;
                                palette[2][2] = (byte)(((palette[0][2] * 2) + palette[1][2]) / 3);
                                palette[2][1] = (byte)(((palette[0][1] * 2) + palette[1][1]) / 3);
                                palette[2][0] = (byte)(((palette[0][0] * 2) + palette[1][0]) / 3);

                                palette[3][3] = 0xFF;
                                palette[3][2] = (byte)(((palette[1][2] * 2) + palette[0][2]) / 3);
                                palette[3][1] = (byte)(((palette[1][1] * 2) + palette[0][1]) / 3);
                                palette[3][0] = (byte)(((palette[1][0] * 2) + palette[0][0]) / 3);
                            }
                            else
                            {
                                palette[2][3] = 0xFF;
                                palette[2][2] = (byte)((palette[0][2] + palette[1][2]) / 2);
                                palette[2][1] = (byte)((palette[0][1] + palette[1][1]) / 2);
                                palette[2][0] = (byte)((palette[0][0] + palette[1][0]) / 2);

                                palette[3][3] = 0x00;
                                palette[3][2] = 0x00;
                                palette[3][1] = 0x00;
                                palette[3][0] = 0x00;
                            }

                            offset += 4;

                            for (int y3 = 0; y3 < 4; y3++)
                            {
                                for (int x3 = 0; x3 < 4; x3++)
                                {
                                    output[((((y + y2 + y3) * Width) + (x + x2 + x3)) * 4) + 3] = palette[((in_Input[offset] >> (6 - (x3 * 2))) & 0x03)][3];
                                    output[((((y + y2 + y3) * Width) + (x + x2 + x3)) * 4) + 2] = palette[((in_Input[offset] >> (6 - (x3 * 2))) & 0x03)][2];
                                    output[((((y + y2 + y3) * Width) + (x + x2 + x3)) * 4) + 1] = palette[((in_Input[offset] >> (6 - (x3 * 2))) & 0x03)][1];
                                    output[((((y + y2 + y3) * Width) + (x + x2 + x3)) * 4) + 0] = palette[((in_Input[offset] >> (6 - (x3 * 2))) & 0x03)][0];
                                }

                                offset++;
                            }
                        }
                    }
                }
            }

            return output;
        }

        public override byte[] Encode(byte[] in_Input)
        {
            byte[] output = new byte[EncodedDataLength];
            int offset = 0;

            byte[] subBlock = new byte[64];

            for (int y = 0; y < Height; y += 8)
            {
                for (int x = 0; x < Width; x += 8)
                {
                    for (int y2 = 0; y2 < 8; y2 += 4)
                    {
                        for (int x2 = 0; x2 < 8; x2 += 4)
                        {
                            int i = 0;

                            for (int y3 = 0; y3 < 4; y3++)
                            {
                                for (int x3 = 0; x3 < 4; x3++)
                                {
                                    subBlock[i + 3] = in_Input[((((y + y2 + y3) * Width) + (x + x2 + x3)) * 4) + 3];
                                    subBlock[i + 2] = in_Input[((((y + y2 + y3) * Width) + (x + x2 + x3)) * 4) + 2];
                                    subBlock[i + 1] = in_Input[((((y + y2 + y3) * Width) + (x + x2 + x3)) * 4) + 1];
                                    subBlock[i + 0] = in_Input[((((y + y2 + y3) * Width) + (x + x2 + x3)) * 4) + 0];

                                    i += 4;
                                }
                            }

                            ConvertBlockToQuaterCmpr(subBlock).CopyTo(output, offset);
                            offset += 8;
                        }
                    }
                }
            }

            return output;
        }

        #region Methods from CTools Wii

        private static byte[] ConvertBlockToQuaterCmpr(byte[] in_Block)
        {
            int col1, col2, dist, temp;
            bool alpha;
            byte[][] palette;
            byte[] result;

            dist = col1 = col2 = -1;
            alpha = false;
            result = new byte[8];

            for (int i = 0; i < 15; i++)
            {
                if (in_Block[i * 4 + 3] < 16)
                    alpha = true;
                else
                {
                    for (int j = i + 1; j < 16; j++)
                    {
                        temp = Distance(in_Block, i * 4, in_Block, j * 4);

                        if (temp > dist)
                        {
                            dist = temp;
                            col1 = i;
                            col2 = j;
                        }
                    }
                }
            }

            if (dist == -1)
            {
                palette = new byte[][] { new byte[] { 0, 0, 0, 0xff }, new byte[] { 0xff, 0xff, 0xff, 0xff }, null, null };
            }
            else
            {
                palette = new byte[4][];
                palette[0] = new byte[4];
                palette[1] = new byte[4];

                Array.Copy(in_Block, col1 * 4, palette[0], 0, 3);
                palette[0][3] = 0xff;
                Array.Copy(in_Block, col2 * 4, palette[1], 0, 3);
                palette[1][3] = 0xff;

                if (palette[0][0] >> 3 == palette[1][0] >> 3 && palette[0][1] >> 2 == palette[1][1] >> 2 && palette[0][2] >> 3 == palette[1][2] >> 3)
                    if (palette[0][0] >> 3 == 0 && palette[0][1] >> 2 == 0 && palette[0][2] >> 3 == 0)
                        palette[1][0] = palette[1][1] = palette[1][2] = 0xff;
                    else
                        palette[1][0] = palette[1][1] = palette[1][2] = 0x0;
            }

            result[0] = (byte)(palette[0][2] & 0xf8 | palette[0][1] >> 5);
            result[1] = (byte)(palette[0][1] << 3 & 0xe0 | palette[0][0] >> 3);
            result[2] = (byte)(palette[1][2] & 0xf8 | palette[1][1] >> 5);
            result[3] = (byte)(palette[1][1] << 3 & 0xe0 | palette[1][0] >> 3);

            if ((result[0] > result[2] || (result[0] == result[2] && result[1] >= result[3])) == alpha)
            {
                Array.Copy(result, 0, result, 4, 2);
                Array.Copy(result, 2, result, 0, 2);
                Array.Copy(result, 4, result, 2, 2);

                palette[2] = palette[0];
                palette[0] = palette[1];
                palette[1] = palette[2];
            }

            if (!alpha)
            {
                palette[2] = new byte[] { (byte)(((palette[0][0] << 1) + palette[1][0]) / 3), (byte)(((palette[0][1] << 1) + palette[1][1]) / 3), (byte)(((palette[0][2] << 1) + palette[1][2]) / 3), 0xff };
                palette[3] = new byte[] { (byte)((palette[0][0] + (palette[1][0] << 1)) / 3), (byte)((palette[0][1] + (palette[1][1] << 1)) / 3), (byte)((palette[0][2] + (palette[1][2] << 1)) / 3), 0xff };
            }
            else
            {
                palette[2] = new byte[] { (byte)((palette[0][0] + palette[1][0]) >> 1), (byte)((palette[0][1] + palette[1][1]) >> 1), (byte)((palette[0][2] + palette[1][2]) >> 1), 0xff };
                palette[3] = new byte[] { 0, 0, 0, 0 };
            }

            for (int i = 0; i < in_Block.Length >> 4; i++)
            {
                result[4 + i] = (byte)(LeastDistance(palette, in_Block, i * 16 + 0) << 6 | LeastDistance(palette, in_Block, i * 16 + 4) << 4 | LeastDistance(palette, in_Block, i * 16 + 8) << 2 | LeastDistance(palette, in_Block, i * 16 + 12));
            }

            return result;
        }

        private static int LeastDistance(byte[][] in_Palette, byte[] in_Colour, int in_Offset)
        {
            int dist, best, temp;

            if (in_Colour[in_Offset + 3] < 8)
                return 3;

            dist = int.MaxValue;
            best = 0;

            for (int i = 0; i < in_Palette.Length; i++)
            {
                if (in_Palette[i][3] != 0xff)
                    break;

                temp = Distance(in_Palette[i], 0, in_Colour, in_Offset);

                if (temp < dist)
                {
                    if (temp == 0)
                        return i;

                    dist = temp;
                    best = i;
                }
            }

            return best;
        }

        private static int Distance(byte[] in_Colour1, int in_Offset1, byte[] in_Colour2, int in_Offset2)
        {
            int temp, val;

            temp = 0;

            for (int i = 0; i < 3; i++)
            {
                val = in_Colour1[in_Offset1 + i] - in_Colour2[in_Offset2 + i];
                temp += val * val;
            }

            return temp;
        }

        #endregion
    }
}