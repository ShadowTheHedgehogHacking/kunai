using System;

namespace Shuriken.Rendering.Gvr
{
    internal abstract class GvrImageDataFormat : ImageDataFormatBase
    {
        public GvrImageDataFormat(ushort in_Width, ushort in_Height) : base(in_Width, in_Height)
        {

        }

        public static GvrImageDataFormat Get(ushort in_Width, ushort in_Height, GvrDataFormat in_Format)
        {
            switch (in_Format)
            {
                case GvrDataFormat.Index4:
                    return new GvrImageDataFormatIndex4(in_Width, in_Height);
                case GvrDataFormat.Index8:
                    return new GvrImageDataFormatIndex8(in_Width, in_Height);
                case GvrDataFormat.Rgb5A3:
                    return new GvrImageDataFormatRgb5A3(in_Width, in_Height);
                case GvrDataFormat.Argb8888:
                    return new GvrImageDataFormatArgb8888(in_Width, in_Height);
                case GvrDataFormat.Dxt1:
                    return new GvrImageDataFormatDxt1(in_Width, in_Height);
                default:
                    throw new NotImplementedException($"Unsupported GVR image data format: {in_Format}.");
            }
        }
    }
}