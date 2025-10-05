using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.GMath
{
    public struct Vector3f : IComparable<Vector3f>, IEquatable<Vector3f>
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3f(float f) { X = Y = Z = f; }
        public Vector3f(float x, float y, float z) { this.X = x; this.Y = y; this.Z = z; }
        public Vector3f(float[] v2) { X = v2[0]; Y = v2[1]; Z = v2[2]; }
        public Vector3f(Vector3f copy) { X = copy.X; Y = copy.Y; Z = copy.Z; }

        public Vector3f(double f) { X = Y = Z = (float)f; }
        public Vector3f(double x, double y, double z) { this.X = (float)x; this.Y = (float)y; this.Z = (float)z; }
        public Vector3f(double[] v2) { X = (float)v2[0]; Y = (float)v2[1]; Z = (float)v2[2]; }

        static public readonly Vector3f Zero = new Vector3f(0.0f, 0.0f, 0.0f);
        static public readonly Vector3f One = new Vector3f(1.0f, 1.0f, 1.0f);
        static public readonly Vector3f OneNormalized = new Vector3f(1.0f, 1.0f, 1.0f).Normalized;
        static public readonly Vector3f Invalid = new Vector3f(float.MaxValue, float.MaxValue, float.MaxValue);
        static public readonly Vector3f AxisX = new Vector3f(1.0f, 0.0f, 0.0f);
        static public readonly Vector3f AxisY = new Vector3f(0.0f, 1.0f, 0.0f);
        static public readonly Vector3f AxisZ = new Vector3f(0.0f, 0.0f, 1.0f);
        static public readonly Vector3f MaxValue = new Vector3f(float.MaxValue, float.MaxValue, float.MaxValue);
        static public readonly Vector3f MinValue = new Vector3f(float.MinValue, float.MinValue, float.MinValue);

        public float this[int key]
        {
            get { return (key == 0) ? X : (key == 1) ? Y : Z; }
            set { if (key == 0) X = value; else if (key == 1) Y = value; else Z = value; }
        }

        public float LengthSquared
        {
            get { return X * X + Y * Y + Z * Z; }
        }
        public float Length
        {
            get { return (float)Math.Sqrt(LengthSquared); }
        }

        public float LengthL1
        {
            get { return Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z); }
        }

        public float Max
        {
            get { return Math.Max(X, Math.Max(Y, Z)); }
        }
        public float Min
        {
            get { return Math.Min(X, Math.Min(Y, Z)); }
        }
        public float MaxAbs
        {
            get { return Math.Max(Math.Abs(X), Math.Max(Math.Abs(Y), Math.Abs(Z))); }
        }
        public float MinAbs
        {
            get { return Math.Min(Math.Abs(X), Math.Min(Math.Abs(Y), Math.Abs(Z))); }
        }


        public float Normalize(float epsilon = MathUtil.Epsilonf)
        {
            float length = Length;
            if (length > epsilon)
            {
                float invLength = 1.0f / length;
                X *= invLength;
                Y *= invLength;
                Z *= invLength;
            }
            else
            {
                length = 0;
                X = Y = Z = 0;
            }
            return length;
        }
        public Vector3f Normalized
        {
            get
            {
                float length = Length;
                if (length > MathUtil.Epsilonf)
                {
                    float invLength = 1 / length;
                    return new Vector3f(X * invLength, Y * invLength, Z * invLength);
                }
                else
                    return Vector3f.Zero;
            }
        }

        public bool IsNormalized
        {
            get { return Math.Abs((X * X + Y * Y + Z * Z) - 1) < MathUtil.ZeroTolerancef; }
        }

        public bool IsFinite
        {
            get { float f = X + Y + Z; return float.IsNaN(f) == false && float.IsInfinity(f) == false; }
        }


        public void Round(int nDecimals)
        {
            X = (float)Math.Round(X, nDecimals);
            Y = (float)Math.Round(Y, nDecimals);
            Z = (float)Math.Round(Z, nDecimals);
        }


        public float Dot(Vector3f v2)
        {
            return X * v2[0] + Y * v2[1] + Z * v2[2];
        }
        public static float Dot(Vector3f v1, Vector3f v2)
        {
            return v1.Dot(v2);
        }


        public Vector3f Cross(Vector3f v2)
        {
            return new Vector3f(
                Y * v2.Z - Z * v2.Y,
                Z * v2.X - X * v2.Z,
                X * v2.Y - Y * v2.X);
        }
        public static Vector3f Cross(Vector3f v1, Vector3f v2)
        {
            return v1.Cross(v2);
        }

        public Vector3f UnitCross(Vector3f v2)
        {
            Vector3f n = new Vector3f(
                Y * v2.Z - Z * v2.Y,
                Z * v2.X - X * v2.Z,
                X * v2.Y - Y * v2.X);
            n.Normalize();
            return n;
        }

        public float AngleD(Vector3f v2)
        {
            float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return (float)(Math.Acos(fDot) * MathUtil.Rad2Deg);
        }
        public static float AngleD(Vector3f v1, Vector3f v2)
        {
            return v1.AngleD(v2);
        }
        public float AngleR(Vector3f v2)
        {
            float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return (float)(Math.Acos(fDot));
        }
        public static float AngleR(Vector3f v1, Vector3f v2)
        {
            return v1.AngleR(v2);
        }


        public float DistanceSquared(Vector3f v2)
        {
            float dx = v2.X - X, dy = v2.Y - Y, dz = v2.Z - Z;
            return dx * dx + dy * dy + dz * dz;
        }
        public float Distance(Vector3f v2)
        {
            float dx = v2.X - X, dy = v2.Y - Y, dz = v2.Z - Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }



        public void Set(Vector3f o)
        {
            X = o[0]; Y = o[1]; Z = o[2];
        }
        public void Set(float fX, float fY, float fZ)
        {
            X = fX; Y = fY; Z = fZ;
        }
        public void Add(Vector3f o)
        {
            X += o[0]; Y += o[1]; Z += o[2];
        }
        public void Subtract(Vector3f o)
        {
            X -= o[0]; Y -= o[1]; Z -= o[2];
        }



        public static Vector3f operator -(Vector3f v)
        {
            return new Vector3f(-v.X, -v.Y, -v.Z);
        }

        public static Vector3f operator *(float f, Vector3f v)
        {
            return new Vector3f(f * v.X, f * v.Y, f * v.Z);
        }
        public static Vector3f operator *(Vector3f v, float f)
        {
            return new Vector3f(f * v.X, f * v.Y, f * v.Z);
        }
        public static Vector3f operator /(Vector3f v, float f)
        {
            return new Vector3f(v.X / f, v.Y / f, v.Z / f);
        }
        public static Vector3f operator /(float f, Vector3f v)
        {
            return new Vector3f(f / v.X, f / v.Y, f / v.Z);
        }

        public static Vector3f operator *(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }
        public static Vector3f operator /(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }


        public static Vector3f operator +(Vector3f v0, Vector3f v1)
        {
            return new Vector3f(v0.X + v1.X, v0.Y + v1.Y, v0.Z + v1.Z);
        }
        public static Vector3f operator +(Vector3f v0, float f)
        {
            return new Vector3f(v0.X + f, v0.Y + f, v0.Z + f);
        }

        public static Vector3f operator -(Vector3f v0, Vector3f v1)
        {
            return new Vector3f(v0.X - v1.X, v0.Y - v1.Y, v0.Z - v1.Z);
        }
        public static Vector3f operator -(Vector3f v0, float f)
        {
            return new Vector3f(v0.X - f, v0.Y - f, v0.Z - f);
        }


        public static bool operator ==(Vector3f a, Vector3f b)
        {
            return (a.X == b.X && a.Y == b.Y && a.Z == b.Z);
        }
        public static bool operator !=(Vector3f a, Vector3f b)
        {
            return (a.X != b.X || a.Y != b.Y || a.Z != b.Z);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector3f)obj;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ X.GetHashCode();
                hash = (hash * 16777619) ^ Y.GetHashCode();
                hash = (hash * 16777619) ^ Z.GetHashCode();
                return hash;
            }
        }
        public int CompareTo(Vector3f other)
        {
            if (X != other.X)
                return X < other.X ? -1 : 1;
            else if (Y != other.Y)
                return Y < other.Y ? -1 : 1;
            else if (Z != other.Z)
                return Z < other.Z ? -1 : 1;
            return 0;
        }
        public bool Equals(Vector3f other)
        {
            return (X == other.X && Y == other.Y && Z == other.Z);
        }


        public bool EpsilonEqual(Vector3f v2, float epsilon)
        {
            return (float)Math.Abs(X - v2.X) <= epsilon &&
                   (float)Math.Abs(Y - v2.Y) <= epsilon &&
                   (float)Math.Abs(Z - v2.Z) <= epsilon;
        }


        public static Vector3f Lerp(Vector3f a, Vector3f b, float t)
        {
            float s = 1 - t;
            return new Vector3f(s * a.X + t * b.X, s * a.Y + t * b.Y, s * a.Z + t * b.Z);
        }



        public override string ToString()
        {
            return string.Format("{0:F8} {1:F8} {2:F8}", X, Y, Z);
        }
        public string ToString(string fmt)
        {
            return string.Format("{0} {1} {2}", X.ToString(fmt), Y.ToString(fmt), Z.ToString(fmt));
        }
    }
}
