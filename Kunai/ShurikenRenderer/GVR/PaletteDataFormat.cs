using System.IO;

namespace Shuriken.Rendering.Gvr
{
    internal abstract class PaletteDataFormat
    {
        public ushort PaletteEntryCount { get; set; }

        public abstract uint DecodedDataLength { get; }
        public abstract uint EncodedDataLength { get; }

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

        public PaletteDataFormat(ushort in_PaletteEntryCount)
        {
            PaletteEntryCount = in_PaletteEntryCount;
        }
    }
}