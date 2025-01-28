using System.IO;
using System.Text.Json;

//Taken from GVRTool, which in turn takes it from Puyotools
namespace Shuriken.Rendering.Gvr
{
    internal class GvrMetadata
    {
        public uint MetadataVersion { get; set; }

        public uint GlobalIndex { get; set; }
        public uint Unknown1 { get; set; }

        public GvrPixelFormat PixelFormat { get; set; }
        public GvrDataFlags DataFlags { get; set; }
        public GvrDataFormat DataFormat { get; set; }

        public GvrPixelFormat PalettePixelFormat { get; set; }
        public ushort PaletteEntryCount { get; set; }

        public byte ExternalPaletteUnknown1 { get; set; }
        public ushort ExternalPaletteUnknown2 { get; set; }
        public ushort ExternalPaletteUnknown3 { get; set; }

        private const uint METADATA_VERSION = 2;

    }
}