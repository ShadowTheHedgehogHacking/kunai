using System;
using System.Numerics;

namespace Kunai
{
    public static class KunaiMath
    {
        public static bool SameSide(Vector2 in_A, Vector2 in_B, Vector2 in_C, Vector2 in_P)
        {
            Vector2 ab = in_B - in_A;
            Vector2 ac = in_C - in_A;
            Vector2 ap = in_P - in_A;

            float cross1 = ab.X * ac.Y - ab.Y * ac.X;
            float cross2 = ab.X * ap.Y - ab.Y * ap.X;

            return Math.Sign(cross1) == Math.Sign(cross2);
        }

        public static Vector2 CenterOfRect(Vector2 in_TopLeft, Vector2 in_TopRight, Vector2 in_BtmRight, Vector2 in_BtmLeft)
        {
            return new Vector2(
                (in_TopLeft.X + in_BtmRight.X + in_TopRight.X + in_BtmLeft.X) / 4,
                (in_TopLeft.Y + in_BtmRight.Y + in_TopRight.Y + in_BtmLeft.Y) / 4
            );

        }
        /// <summary>
        /// Checks if a point is within a rect made up of 4 Vector2s.
        /// </summary>
        /// <param name="in_Point">Point to check</param>
        /// <param name="in_RTopLeft">Top left of rect</param>
        /// <param name="in_RTopRight">Top right of rect</param>
        /// <param name="in_RBtmRight">Bottom right of rect</param>
        /// <param name="in_RBtmLeft">Bottom left of rect</param>
        /// <returns></returns>
        public static bool IsPointInRect(Vector2 in_Point, Vector2 in_RTopLeft, Vector2 in_RTopRight, Vector2 in_RBtmRight, Vector2 in_RBtmLeft)
        {
            return SameSide(in_RTopLeft, in_RTopRight, in_RBtmRight, in_Point) && SameSide(in_RTopRight, in_RBtmRight, in_RBtmLeft, in_Point) &&
                   SameSide(in_RBtmRight, in_RBtmLeft, in_RTopLeft, in_Point) && SameSide(in_RBtmLeft, in_RTopLeft, in_RTopRight, in_Point);
        }
    }
}
