using System;
using System.Collections.Generic;

namespace RLP
{
    public struct Vector2
    {
        public double X;
        public double Y;

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2 { X = a.X + b.X, Y = a.Y + b.Y };
        }
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2 { X = a.X - b.X, Y = a.Y - b.Y };
        }
        public static Vector2 operator *(Vector2 a, double scalar)
        {
            return new Vector2 { X = a.X * scalar, Y = a.Y * scalar };
        }

        public static Vector2 operator /(Vector2 a, double scalar)
        {
            return new Vector2 { X = a.X / scalar, Y = a.Y / scalar };
        }

        public static Vector2 operator ^(Vector2 a, Vector2 b)
        {
            throw new NotImplementedException();
        }
        public static double operator *(Vector2 a, Vector2 b)
        {
            return (a.X * b.X + b.Y * b.Y)*(1.0/(a-b).Module());
        }

        public double Module()
        {
            return Math.Sqrt(X * X + Y * Y);
        }
        public override string ToString()
        {
            return "X = " + X + " Y =" + Y;
        }

        internal Vector2 Normalize()
        {
            double module = this.Module();
            return new Vector2 { X = X / module, Y = Y / module };
        }
    }
}