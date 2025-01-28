using System;

namespace Shuriken.Rendering.Gvr
{
    //Modified from GVRTool
    internal abstract class GvrPaletteDataFormat : PaletteDataFormat
    {
        public GvrPaletteDataFormat(ushort in_PaletteEntryCount) : base(in_PaletteEntryCount)
        {

        }

        public static GvrPaletteDataFormat Get(ushort in_PaletteEntryCount, GvrPixelFormat in_Format)
        {
            switch (in_Format)
            {
                case GvrPixelFormat.Rgb5A3:
                    return new GvrPaletteA8R8G8B8(in_PaletteEntryCount);
                case GvrPixelFormat.Rgb565:
                    return new GvrPaletteR8G8B8(in_PaletteEntryCount);
                default:
                    throw new NotImplementedException($"Unsupported GVR palette data format: {in_Format}.");
            }
        }
    }
}