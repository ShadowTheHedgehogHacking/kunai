public static class ExtensionKillMe
{
    public static System.Numerics.Vector4 ToVec4(this Color<byte> value)
    {
        return new System.Numerics.Vector4(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f, value.A / 255.0f);
    }
    public static Color<byte> ToSharpNeedleColor(this System.Numerics.Vector4 value)
    {
        return new Color<byte>((byte)(value.X * 255), (byte)(value.Y * 255), (byte)(value.Z * 255), (byte)(value.W * 255));
    }
}