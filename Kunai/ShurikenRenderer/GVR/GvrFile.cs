using System;
using System.IO;

namespace Shuriken.Rendering.Gvr
{
    //Taken from GVRTool, which in turn gets it from puyotools
    // https://github.com/nickworonekin/puyotools

    // Gvr Pixel Formats
    public enum GvrPixelFormat : byte
    {
        IntensityA8 = 0x00,
        Rgb565 = 0x01,
        Rgb5A3 = 0x02,
        Unknown = 0xFF,
    }

    // Gvr Data Formats
    public enum GvrDataFormat : byte
    {
        Intensity4 = 0x00,
        Intensity8 = 0x01,
        IntensityA4 = 0x02,
        IntensityA8 = 0x03,
        Rgb565 = 0x04,
        Rgb5A3 = 0x05,
        Argb8888 = 0x06,
        Index4 = 0x08,
        Index8 = 0x09,
        Dxt1 = 0x0E,
        Unknown = 0xFF,
    }

    // Gvr Data Flags
    [Flags]
    public enum GvrDataFlags : byte
    {
        None = 0x0,
        Mipmaps = 0x1,
        ExternalPalette = 0x2,
        InternalPalette = 0x8,
        Palette = ExternalPalette | InternalPalette,
    }
    public static class Util
    {
        public static ushort SwapU16(ushort in_Value)
        {
            return (ushort)((in_Value << 8) | (in_Value >> 8));
        }

        public static uint SwapU32(uint in_Value)
        {
            return (in_Value << 24) | ((in_Value & 0x0000FF00) << 8) | ((in_Value & 0x00FF0000) >> 8) | (in_Value >> 24);
        }
        public static uint ReadUInt32Endian(this BinaryReader in_Br, bool in_BigEndian)
        {
            if (in_BigEndian) return SwapU32(in_Br.ReadUInt32());
            else return in_Br.ReadUInt32();
        }
        public static ushort ReadUInt16Endian(this BinaryReader in_Br, bool in_BigEndian)
        {
            if (in_BigEndian) return SwapU16(in_Br.ReadUInt16());
            else return in_Br.ReadUInt16();
        }
    }
    public class GvrFile
    {
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }

        public uint GlobalIndex { get; private set; }
        public uint Unknown1 { get; private set; }

        public GvrPixelFormat PixelFormat { get; private set; }
        public GvrDataFlags DataFlags { get; private set; }
        public GvrDataFormat DataFormat { get; private set; }

        public GvrPixelFormat PalettePixelFormat { get; private set; }
        public ushort PaletteEntryCount { get; private set; }

        public byte ExternalPaletteUnknown1 { get; private set; }
        public ushort ExternalPaletteUnknown2 { get; private set; }
        public ushort ExternalPaletteUnknown3 { get; private set; }

        public ushort BitsPerPixel { get; private set; }
        public byte[] Pixels { get; private set; }
        public byte[] Palette { get; private set; }

        // Helpers
        public bool HasExternalPalette => (DataFlags & GvrDataFlags.ExternalPalette) != 0;
        public bool HasInternalPalette => (DataFlags & GvrDataFlags.InternalPalette) != 0;
        public bool HasPalette => (DataFlags & GvrDataFlags.Palette) != 0;
        public bool HasMipmaps => (DataFlags & GvrDataFlags.Mipmaps) != 0;

        private bool _isLoaded;

        private const uint GcixMagic = 0x58494347;
        private const uint GvrtMagic = 0x54525647;
        private const uint GvplMagic = 0x4c505647;

        private const bool BigEndian = true;

        public GvrFile()
        {
            _isLoaded = false;
        }

        public void LoadFromGvrFile(string in_GvrPath)
        {
            if (string.IsNullOrWhiteSpace(in_GvrPath))
            {
                throw new ArgumentNullException(nameof(in_GvrPath));
            }

            if (!File.Exists(in_GvrPath))
            {
                throw new FileNotFoundException($"GVR file has not been found: {in_GvrPath}.");
            }

            using (FileStream fs = File.OpenRead(in_GvrPath))
            using (BinaryReader br = new BinaryReader(fs))
            {
                uint gcixMagic = br.ReadUInt32();
                if (gcixMagic != GcixMagic)
                {
                    throw new InvalidDataException($"\"{in_GvrPath}\" is not a valid GCIX/GVRT file.");
                }

                fs.Position = 0x10;

                uint gvrtMagic = br.ReadUInt32();
                if (gvrtMagic != GvrtMagic)
                {
                    throw new InvalidDataException($"\"{in_GvrPath}\" is not a valid GCIX/GVRT file.");
                }

                fs.Position = 0x8;
                GlobalIndex = br.ReadUInt32Endian(BigEndian);
                Unknown1 = br.ReadUInt32Endian(BigEndian);

                fs.Position = 0x1A;
                byte pixelFormatAndFlags = br.ReadByte();
                PixelFormat = (GvrPixelFormat)(pixelFormatAndFlags >> 4);
                DataFlags = (GvrDataFlags)(pixelFormatAndFlags & 0x0F);
                DataFormat = (GvrDataFormat)br.ReadByte();
                Width = br.ReadUInt16Endian(BigEndian);
                Height = br.ReadUInt16Endian(BigEndian);

                if (HasMipmaps)
                {
                    throw new NotImplementedException($"Textures with mip maps are not supported.");
                }

                GvrImageDataFormat format = GvrImageDataFormat.Get(Width, Height, DataFormat);
                BitsPerPixel = (ushort)format.BitsPerPixel;
                Pixels = format.Decode(fs);
            }

            if (HasExternalPalette)
            {
                string gvpPath = Path.ChangeExtension(in_GvrPath, ".gvp");

                if (!File.Exists(gvpPath))
                {
                    throw new FileNotFoundException($"External GVP palette has not been found: {gvpPath}.");
                }

                using (FileStream fs = File.OpenRead(gvpPath))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    uint gvplMagic = br.ReadUInt32();
                    if (gvplMagic != GvplMagic)
                    {
                        throw new InvalidDataException($"\"{gvpPath}\" is not a valid GVPL file.");
                    }

                    fs.Position = 0x8;
                    ExternalPaletteUnknown1 = br.ReadByte();
                    PalettePixelFormat = (GvrPixelFormat)br.ReadByte();
                    ExternalPaletteUnknown2 = br.ReadUInt16Endian(BigEndian);
                    ExternalPaletteUnknown3 = br.ReadUInt16Endian(BigEndian);
                    PaletteEntryCount = br.ReadUInt16Endian(BigEndian);

                    GvrPaletteDataFormat format = GvrPaletteDataFormat.Get(PaletteEntryCount, PalettePixelFormat);
                    Palette = format.Decode(fs);
                }
            }

            _isLoaded = true;
        }
    }
}