using DirectXTexNet;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;

public static class BitmapConverter
{
    public static BitmapSource FromBitmap(Bitmap in_Bitmap)
    {
        BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                       in_Bitmap.GetHbitmap(),
                       nint.Zero,
                       Int32Rect.Empty,
                       BitmapSizeOptions.FromEmptyOptions());
        return i;
    }

    public static Bitmap FromTextureImage(ScratchImage in_Img, System.Drawing.Imaging.PixelFormat in_Format)
    {
        return new Bitmap(in_Img.GetImage(0).Width, in_Img.GetImage(0).Height,
            (int)in_Img.GetImage(0).RowPitch, in_Format, in_Img.GetImage(0).Pixels);
    }
}