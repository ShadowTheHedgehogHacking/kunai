using Hexa.NET.ImGui;
using Kunai.ShurikenRenderer;
using Kunai.Window;
using SharpNeedle.Framework.Ninja.Csd;
using System;
using System.Numerics;

using CsdCast = SharpNeedle.Framework.Ninja.Csd.Cast;
namespace Kunai
{
    public static class PivotHelper
    {
        static EAlignmentPivot InvertPivot(EAlignmentPivot in_Pivot)
        {
            EAlignmentPivot newPivot = EAlignmentPivot.None;
            switch (in_Pivot)
            {
                case EAlignmentPivot.TopLeft:
                    newPivot = EAlignmentPivot.BottomRight;
                    break;
                case EAlignmentPivot.TopCenter:
                    newPivot = EAlignmentPivot.BottomCenter;
                    break;
                case EAlignmentPivot.TopRight:
                    newPivot = EAlignmentPivot.BottomLeft;
                    break;
                case EAlignmentPivot.MiddleLeft:
                    newPivot = EAlignmentPivot.MiddleRight;
                    break;
                case EAlignmentPivot.MiddleCenter:
                    newPivot = EAlignmentPivot.MiddleCenter;
                    break;
                case EAlignmentPivot.MiddleRight:
                    newPivot = EAlignmentPivot.MiddleLeft;
                    break;
                case EAlignmentPivot.BottomLeft:
                    newPivot = EAlignmentPivot.TopRight;
                    break;
                case EAlignmentPivot.BottomCenter:
                    newPivot = EAlignmentPivot.TopCenter;
                    break;
                case EAlignmentPivot.BottomRight:
                    newPivot = EAlignmentPivot.TopLeft;
                    break;
            }
            return newPivot;
        }
        static EAlignmentPivot GetPivot(CsdCast in_Cast)
        {
            var renderer = KunaiProject.Instance;

            var size = new Vector2(in_Cast.Width, in_Cast.Height) * 2;
            var topLeft = in_Cast.TopLeft / (size / renderer.ViewportSize);
            var bottomRight = in_Cast.BottomRight / (size / renderer.ViewportSize);
            var topRight = in_Cast.TopRight / (size / renderer.ViewportSize);
            var bottomLeft = in_Cast.BottomLeft / (size / renderer.ViewportSize);
            EAlignmentPivot returnValue = EAlignmentPivot.None;
            if (topLeft == new Vector2(-1, -1) && topRight == new Vector2(0, -1) && bottomLeft == new Vector2(-1, 0) && bottomRight == new Vector2(0, 0))
                returnValue = EAlignmentPivot.TopLeft;

            if (topLeft == new Vector2(-0.5f, -1) && topRight == new Vector2(0.5f, -1) && bottomLeft == new Vector2(-0.5f, 0) && bottomRight == new Vector2(0.5f, 0))
                returnValue = EAlignmentPivot.TopCenter;

            if (topLeft == new Vector2(0, -1) && topRight == new Vector2(1, -1) && bottomLeft == new Vector2(0, 0) && bottomRight == new Vector2(1, 0))
                returnValue = EAlignmentPivot.TopRight;

            if (topLeft == new Vector2(-1, -0.5f) && topRight == new Vector2(0, -0.5f) && bottomLeft == new Vector2(-1, 0.5f) && bottomRight == new Vector2(0, 0.5f))
                returnValue = EAlignmentPivot.MiddleLeft;

            if (topLeft == new Vector2(-0.5f, -0.5f) && topRight == new Vector2(0.5f, -0.5f) && bottomLeft == new Vector2(-0.5f, 0.5f) && bottomRight == new Vector2(0.5f, 0.5f))
                returnValue = EAlignmentPivot.MiddleCenter;

            if (topLeft == new Vector2(0, -0.5f) && topRight == new Vector2(1, -0.5f) && bottomLeft == new Vector2(0, 0.5f) && bottomRight == new Vector2(1, 0.5f))
                returnValue = EAlignmentPivot.MiddleRight;

            if (topLeft == new Vector2(-1, 0) && topRight == new Vector2(0, 0) && bottomLeft == new Vector2(-1, 1) && bottomRight == new Vector2(0, 1))
                returnValue = EAlignmentPivot.BottomLeft;

            if (topLeft == new Vector2(-0.5f, 0) && topRight == new Vector2(0.5f, 0) && bottomLeft == new Vector2(-0.5f, 1) && bottomRight == new Vector2(0.5f, 1))
                returnValue = EAlignmentPivot.BottomCenter;

            if (topLeft == new Vector2(0, 0) && topRight == new Vector2(1, 0) && bottomLeft == new Vector2(0, 1) && bottomRight == new Vector2(1, 1))
                returnValue = EAlignmentPivot.BottomRight;
            return InvertPivot(returnValue);
        }
        private static void AlignQuadTo(EAlignmentPivot in_AlignmentPosition, ref CsdCast in_Cast)
        {
            Vector2 quadCenter = KunaiMath.CenterOfRect(in_Cast.TopLeft, in_Cast.TopRight, in_Cast.BottomRight, in_Cast.BottomLeft);

            var diff1 = in_Cast.Position - quadCenter;
            Vector2 topLeft = new Vector2(0, 0), topRight = new Vector2(0, 0), bottomLeft = new Vector2(0, 0), bottomRight = new Vector2(0, 0);
            Vector2 size = (new Vector2(in_Cast.Width, in_Cast.Height) * 2) / KunaiProject.Instance.ViewportSize;
            switch (InvertPivot(in_AlignmentPosition))
            {
                case EAlignmentPivot.TopLeft:
                    topLeft = new Vector2(-1, -1);
                    topRight = new Vector2(0, -1);
                    bottomLeft = new Vector2(-1, 0);
                    bottomRight = new Vector2(0, 0);
                    break;

                case EAlignmentPivot.TopCenter:
                    topLeft = new Vector2(-0.5f, -1);
                    topRight = new Vector2(0.5f, -1);
                    bottomLeft = new Vector2(-0.5f, 0);
                    bottomRight = new Vector2(0.5f, 0);
                    break;

                case EAlignmentPivot.TopRight:
                    topLeft = new Vector2(0, -1);
                    topRight = new Vector2(1, -1);
                    bottomLeft = new Vector2(0, 0);
                    bottomRight = new Vector2(1, 0);
                    break;

                case EAlignmentPivot.MiddleLeft:
                    topLeft = new Vector2(-1, -0.5f);
                    topRight = new Vector2(0, -0.5f);
                    bottomLeft = new Vector2(-1, 0.5f);
                    bottomRight = new Vector2(0, 0.5f);
                    break;

                case EAlignmentPivot.MiddleCenter:
                    topLeft = new Vector2(-0.5f, -0.5f);
                    topRight = new Vector2(0.5f, -0.5f);
                    bottomLeft = new Vector2(-0.5f, 0.5f);
                    bottomRight = new Vector2(0.5f, 0.5f);
                    break;

                case EAlignmentPivot.MiddleRight:
                    topLeft = new Vector2(0, -0.5f);
                    topRight = new Vector2(1, -0.5f);
                    bottomLeft = new Vector2(0, 0.5f);
                    bottomRight = new Vector2(1, 0.5f);
                    break;

                case EAlignmentPivot.BottomLeft:
                    topLeft = new Vector2(-1, 0);
                    topRight = new Vector2(0, 0);
                    bottomLeft = new Vector2(-1, 1);
                    bottomRight = new Vector2(0, 1);
                    break;

                case EAlignmentPivot.BottomCenter:
                    topLeft = new Vector2(-0.5f, 0);
                    topRight = new Vector2(0.5f, 0);
                    bottomLeft = new Vector2(-0.5f, 1);
                    bottomRight = new Vector2(0.5f, 1);
                    break;

                case EAlignmentPivot.BottomRight:
                    topLeft = new Vector2(0, 0);
                    topRight = new Vector2(1, 0);
                    bottomLeft = new Vector2(0, 1);
                    bottomRight = new Vector2(1, 1);
                    break;
            }
            // Calculate the offset to move the center to the alignment position
            in_Cast.TopLeft = topLeft * size;
            in_Cast.TopRight = topRight * size;
            in_Cast.BottomLeft = bottomLeft * size;
            in_Cast.BottomRight = bottomRight * size;

            Vector2 quadCenter2 = KunaiMath.CenterOfRect(in_Cast.TopLeft, in_Cast.TopRight, in_Cast.BottomRight, in_Cast.BottomLeft);
            var diff2 = in_Cast.Position - quadCenter2;
            var t = in_Cast.Info;
            var diff = (((quadCenter2 - quadCenter)));
            in_Cast.Info = t;
        }
        public static bool DrawAlignmentGridRadio(ref CsdCast in_Cast)
        {
            EAlignmentPivot currentPivot = GetPivot(in_Cast);
            Vector2 quadCenter = KunaiMath.CalculatePivot([in_Cast.TopLeft, in_Cast.TopRight, in_Cast.BottomRight, in_Cast.BottomLeft]);
            //Console.WriteLine(quadCenter);
            bool changed = false;
            if (ImGui.RadioButton("##tl", currentPivot == EAlignmentPivot.TopLeft))
            {
                changed = true;
                AlignQuadTo(EAlignmentPivot.TopLeft, ref in_Cast);
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("##tc", currentPivot == EAlignmentPivot.TopCenter))
            {
                changed = true;
                AlignQuadTo(EAlignmentPivot.TopCenter, ref in_Cast);
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("##tr", currentPivot == EAlignmentPivot.TopRight))
            {
                changed = true;
                AlignQuadTo(EAlignmentPivot.TopRight, ref in_Cast);
            }

            if (ImGui.RadioButton("##ml", currentPivot == EAlignmentPivot.MiddleLeft))
            {
                changed = true;
                AlignQuadTo(EAlignmentPivot.MiddleLeft, ref in_Cast);
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("##mc", currentPivot == EAlignmentPivot.MiddleCenter))
            {
                changed = true;
                AlignQuadTo(EAlignmentPivot.MiddleCenter, ref in_Cast);
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("##mr", currentPivot == EAlignmentPivot.MiddleRight))
            {
                changed = true;
                AlignQuadTo(EAlignmentPivot.MiddleRight, ref in_Cast);
            }

            if (ImGui.RadioButton("##bl", currentPivot == EAlignmentPivot.BottomLeft))
            {
                changed = true;
                AlignQuadTo(EAlignmentPivot.BottomLeft, ref in_Cast);
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("##bc", currentPivot == EAlignmentPivot.BottomCenter))
            {
                changed = true;
                AlignQuadTo(EAlignmentPivot.BottomCenter, ref in_Cast);
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("##br", currentPivot == EAlignmentPivot.BottomRight))
            {
                changed = true;
                AlignQuadTo(EAlignmentPivot.BottomRight, ref in_Cast);
            }
            return changed;
        }
    }
}