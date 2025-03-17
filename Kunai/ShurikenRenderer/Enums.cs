using System;

namespace Shuriken.Rendering
{
    public enum DrawType : uint
    {
        None,
        Sprite,
        Font
    }
    [Flags]
    public enum AnimationType : uint
    {
        None = 0,
        HideFlag = 1,
        XPosition = 2,
        YPosition = 4,
        Rotation = 8,
        XScale = 16,
        YScale = 32,
        SubImage = 64,
        Color = 128,
        GradientTl = 256,
        GradientBl = 512,
        GradientTr = 1024,
        GradientBr = 2048
    }
    [Flags]
    public enum ElementInheritanceFlags
    {
        None = 0,
        InheritRotation = 0x2,
        InheritColor = 0x8,
        InheritXPosition = 0x100,
        InheritYPosition = 0x200,
        InheritScaleX = 0x400,
        InheritScaleY = 0x800,
    }
    [Flags]
    public enum ElementMaterialFlags
    {
        None = 0,
        AdditiveBlending = 0x1,
        MirrorX = 0x400,
        MirrorY = 0x800,
        LinearFiltering = 0x1000
    }
    public enum ElementMaterialFiltering
    {
        NearestNeighbor = 0,
        Linear = 1
    }
    public enum ElementMaterialBlend
    {
        Normal = 0,
        Additive = 1
    }
}
