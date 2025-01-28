using System;
using System.Numerics;

namespace Kunai.ShurikenRenderer
{
    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float in_X = 0.0f, float in_Y = 0.0f)
        {
            X = in_X;
            Y = in_Y;
        }

        public Vector2(Vector2 in_V)
        {
            X = in_V.X;
            Y = in_V.Y;
        }

        public Vector2(System.Numerics.Vector2 in_V)
        {
            X = in_V.X;
            Y = in_V.Y;
        }
        public Vector2(Vector3 in_V)
        {
            X = in_V.X;
            Y = in_V.Y;
        }

        public static implicit operator Vector2(System.Numerics.Vector2 in_D) => new Vector2(in_D);
        public System.Numerics.Vector2 ToSystemNumerics()
        {
            return new System.Numerics.Vector2(X, Y);
        }

        public static Vector2 operator +(Vector2 in_V1, Vector2 in_V2)
        {
            return new Vector2(in_V1.X + in_V2.X, in_V1.Y + in_V2.Y);
        }

        public static Vector2 operator -(Vector2 in_V1, Vector2 in_V2)
        {
            return new Vector2(in_V1.X - in_V2.X, in_V1.Y - in_V2.Y);
        }

        public static Vector2 operator *(Vector2 in_V1, Vector2 in_V2)
        {
            return new Vector2(in_V1.X * in_V2.X, in_V1.Y * in_V2.Y);
        }
        public static Vector2 operator /(Vector2 in_V1, Vector2 in_V2)
        {
            return new Vector2(in_V1.X / in_V2.X, in_V1.Y / in_V2.Y);
        }

        public static implicit operator System.Numerics.Vector2(Vector2 in_V)
        {
            return new System.Numerics.Vector2(in_V.X, in_V.Y);
        }
        public Vector2 Rotate(float in_Angle)
        {
            return new Vector2(
                X * MathF.Cos(in_Angle) + Y * MathF.Sin(in_Angle),
                Y * MathF.Cos(in_Angle) - X * MathF.Sin(in_Angle));
        }
        public override string ToString()
        {
            return $"<{X}, {Y}>";
        }
    }
}