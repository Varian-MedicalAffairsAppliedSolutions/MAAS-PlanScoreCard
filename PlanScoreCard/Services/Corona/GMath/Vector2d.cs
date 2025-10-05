using System;

namespace CoronaDVH.GMath
{
    public struct Vector2d : IComparable<Vector2d>, IEquatable<Vector2d>
    {
        public double X;
        public double Y;

        public Vector2d(double f) { X = Y = f; }
        public Vector2d(double x, double y) { this.X = x; this.Y = y; }
        public Vector2d(double[] v2) { X = v2[0]; Y = v2[1]; }
        public Vector2d(float f) { X = Y = f; }
        public Vector2d(float x, float y) { this.X = x; this.Y = y; }
        public Vector2d(float[] v2) { X = v2[0]; Y = v2[1]; }
        public Vector2d(Vector2d copy) { X = copy.X; Y = copy.Y; }

        static public readonly Vector2d Zero = new Vector2d(0.0f, 0.0f);
        static public readonly Vector2d One = new Vector2d(1.0f, 1.0f);
        static public readonly Vector2d AxisX = new Vector2d(1.0f, 0.0f);
        static public readonly Vector2d AxisY = new Vector2d(0.0f, 1.0f);
        static public readonly Vector2d MaxValue = new Vector2d(double.MaxValue, double.MaxValue);
        static public readonly Vector2d MinValue = new Vector2d(double.MinValue, double.MinValue);

        public static Vector2d FromAngleRad(double angle)
        {
            return new Vector2d(Math.Cos(angle), Math.Sin(angle));
        }
        public static Vector2d FromAngleDeg(double angle)
        {
            angle *= MathUtil.Deg2Rad;
            return new Vector2d(Math.Cos(angle), Math.Sin(angle));
        }


        public double this[int key]
        {
            get { return (key == 0) ? X : Y; }
            set { if (key == 0) X = value; else Y = value; }
        }


        public double LengthSquared
        {
            get { return X * X + Y * Y; }
        }
        public double Length
        {
            get { return (double)Math.Sqrt(LengthSquared); }
        }

        public double Normalize(double epsilon = MathUtil.Epsilon)
        {
            double length = Length;
            if (length > epsilon)
            {
                double invLength = 1.0 / length;
                X *= invLength;
                Y *= invLength;
            }
            else
            {
                length = 0;
                X = Y = 0;
            }
            return length;
        }
        public Vector2d Normalized
        {
            get
            {
                double length = Length;
                if (length > MathUtil.Epsilon)
                {
                    double invLength = 1 / length;
                    return new Vector2d(X * invLength, Y * invLength);
                }
                else
                    return Vector2d.Zero;
            }
        }

        public bool IsNormalized
        {
            get { return Math.Abs((X * X + Y * Y) - 1) < MathUtil.ZeroTolerance; }
        }

        public bool IsFinite
        {
            get { double f = X + Y; return double.IsNaN(f) == false && double.IsInfinity(f) == false; }
        }

        public void Round(int nDecimals)
        {
            X = Math.Round(X, nDecimals);
            Y = Math.Round(Y, nDecimals);
        }


        public double Dot(Vector2d v2)
        {
            return X * v2.X + Y * v2.Y;
        }


        /// <summary>
        /// returns cross-product of this vector with v2 (same as DotPerp)
        /// </summary>
        public double Cross(Vector2d v2)
        {
            return X * v2.Y - Y * v2.X;
        }


        /// <summary>
        /// returns right-perp vector, ie rotated 90 degrees to the right
        /// </summary>
		public Vector2d Perp
        {
            get { return new Vector2d(Y, -X); }
        }

        /// <summary>
        /// returns right-perp vector, ie rotated 90 degrees to the right
        /// </summary>
		public Vector2d UnitPerp
        {
            get { return new Vector2d(Y, -X).Normalized; }
        }

        /// <summary>
        /// returns dot-product of this vector with v2.Perp
        /// </summary>
		public double DotPerp(Vector2d v2)
        {
            return X * v2.Y - Y * v2.X;
        }


        public double AngleD(Vector2d v2)
        {
            double fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return Math.Acos(fDot) * MathUtil.Rad2Deg;
        }
        public static double AngleD(Vector2d v1, Vector2d v2)
        {
            return v1.AngleD(v2);
        }
        public double AngleR(Vector2d v2)
        {
            double fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return Math.Acos(fDot);
        }
        public static double AngleR(Vector2d v1, Vector2d v2)
        {
            return v1.AngleR(v2);
        }

        public double DistanceSquared(Vector2d v2)
        {
            double dx = v2.X - X, dy = v2.Y - Y;
            return dx * dx + dy * dy;
        }
        public double Distance(Vector2d v2)
        {
            double dx = v2.X - X, dy = v2.Y - Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }


        public void Set(Vector2d o)
        {
            X = o.X; Y = o.Y;
        }
        public void Set(double fX, double fY)
        {
            X = fX; Y = fY;
        }
        public void Add(Vector2d o)
        {
            X += o.X; Y += o.Y;
        }
        public void Subtract(Vector2d o)
        {
            X -= o.X; Y -= o.Y;
        }



        public static Vector2d operator -(Vector2d v)
        {
            return new Vector2d(-v.X, -v.Y);
        }

        public static Vector2d operator +(Vector2d a, Vector2d o)
        {
            return new Vector2d(a.X + o.X, a.Y + o.Y);
        }
        public static Vector2d operator +(Vector2d a, double f)
        {
            return new Vector2d(a.X + f, a.Y + f);
        }

        public static Vector2d operator -(Vector2d a, Vector2d o)
        {
            return new Vector2d(a.X - o.X, a.Y - o.Y);
        }
        public static Vector2d operator -(Vector2d a, double f)
        {
            return new Vector2d(a.X - f, a.Y - f);
        }

        public static Vector2d operator *(Vector2d a, double f)
        {
            return new Vector2d(a.X * f, a.Y * f);
        }
        public static Vector2d operator *(double f, Vector2d a)
        {
            return new Vector2d(a.X * f, a.Y * f);
        }
        public static Vector2d operator /(Vector2d v, double f)
        {
            return new Vector2d(v.X / f, v.Y / f);
        }
        public static Vector2d operator /(double f, Vector2d v)
        {
            return new Vector2d(f / v.X, f / v.Y);
        }


        public static Vector2d operator *(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X * b.X, a.Y * b.Y);
        }
        public static Vector2d operator /(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X / b.X, a.Y / b.Y);
        }


        public static bool operator ==(Vector2d a, Vector2d b)
        {
            return (a.X == b.X && a.Y == b.Y);
        }
        public static bool operator !=(Vector2d a, Vector2d b)
        {
            return (a.X != b.X || a.Y != b.Y);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector2d)obj;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ X.GetHashCode();
                hash = (hash * 16777619) ^ Y.GetHashCode();
                return hash;
            }
        }
        public int CompareTo(Vector2d other)
        {
            if (X != other.X)
                return X < other.X ? -1 : 1;
            else if (Y != other.Y)
                return Y < other.Y ? -1 : 1;
            return 0;
        }
        public bool Equals(Vector2d other)
        {
            return (X == other.X && Y == other.Y);
        }


        public bool EpsilonEqual(Vector2d v2, double epsilon)
        {
            return Math.Abs(X - v2.X) <= epsilon &&
                   Math.Abs(Y - v2.Y) <= epsilon;
        }


        public static Vector2d Lerp(Vector2d a, Vector2d b, double t)
        {
            double s = 1 - t;
            return new Vector2d(s * a.X + t * b.X, s * a.Y + t * b.Y);
        }
        public static Vector2d Lerp(ref Vector2d a, ref Vector2d b, double t)
        {
            double s = 1 - t;
            return new Vector2d(s * a.X + t * b.X, s * a.Y + t * b.Y);
        }


        public override string ToString()
        {
            return string.Format("{0:F8} {1:F8}", X, Y);
        }
    }
}