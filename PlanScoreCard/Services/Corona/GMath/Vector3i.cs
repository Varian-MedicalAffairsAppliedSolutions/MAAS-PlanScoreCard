using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.GMath
{
    /// <summary>
    /// 3D integer vector type. This is basically the same as Index3i but
    /// with .x.y.z member names. This makes code far more readable in many places.
    /// Unfortunately I can't see a way to do this w/o so much duplication...we could
    /// have .x/.y/.z accessors but that is much less efficient...
    /// </summary>
    public struct Vector3i : IComparable<Vector3i>, IEquatable<Vector3i>
    {
        public int X;
        public int Y;
        public int Z;

        public Vector3i(int f) { X = Y = Z = f; }
        public Vector3i(int x, int y, int z) { this.X = x; this.Y = y; this.Z = z; }
        public Vector3i(int[] v2) { X = v2[0]; Y = v2[1]; Z = v2[2]; }

        static public readonly Vector3i Zero = new Vector3i(0, 0, 0);
        static public readonly Vector3i One = new Vector3i(1, 1, 1);
        static public readonly Vector3i AxisX = new Vector3i(1, 0, 0);
        static public readonly Vector3i AxisY = new Vector3i(0, 1, 0);
        static public readonly Vector3i AxisZ = new Vector3i(0, 0, 1);

        public int this[int key]
        {
            get { return (key == 0) ? X : (key == 1) ? Y : Z; }
            set { if (key == 0) X = value; else if (key == 1) Y = value; else Z = value; }
        }

        public int[] array
        {
            get { return new int[] { X, Y, Z }; }
        }



        public void Set(Vector3i o)
        {
            X = o.X; Y = o.Y; Z = o.Z;
        }
        public void Set(int fX, int fY, int fZ)
        {
            X = fX; Y = fY; Z = fZ;
        }
        public void Add(Vector3i o)
        {
            X += o.X; Y += o.Y; Z += o.Z;
        }
        public void Subtract(Vector3i o)
        {
            X -= o.X; Y -= o.Y; Z -= o.Z;
        }
        public void Add(int s) { X += s; Y += s; Z += s; }


        public int LengthSquared { get { return X * X + Y * Y + Z * Z; } }


        public static Vector3i operator -(Vector3i v)
        {
            return new Vector3i(-v.X, -v.Y, -v.Z);
        }

        public static Vector3i operator *(int f, Vector3i v)
        {
            return new Vector3i(f * v.X, f * v.Y, f * v.Z);
        }
        public static Vector3i operator *(Vector3i v, int f)
        {
            return new Vector3i(f * v.X, f * v.Y, f * v.Z);
        }
        public static Vector3i operator /(Vector3i v, int f)
        {
            return new Vector3i(v.X / f, v.Y / f, v.Z / f);
        }
        public static Vector3i operator /(int f, Vector3i v)
        {
            return new Vector3i(f / v.X, f / v.Y, f / v.Z);
        }

        public static Vector3i operator *(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }
        public static Vector3i operator /(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }


        public static Vector3i operator +(Vector3i v0, Vector3i v1)
        {
            return new Vector3i(v0.X + v1.X, v0.Y + v1.Y, v0.Z + v1.Z);
        }
        public static Vector3i operator +(Vector3i v0, int f)
        {
            return new Vector3i(v0.X + f, v0.Y + f, v0.Z + f);
        }

        public static Vector3i operator -(Vector3i v0, Vector3i v1)
        {
            return new Vector3i(v0.X - v1.X, v0.Y - v1.Y, v0.Z - v1.Z);
        }
        public static Vector3i operator -(Vector3i v0, int f)
        {
            return new Vector3i(v0.X - f, v0.Y - f, v0.Z - f);
        }




        public static bool operator ==(Vector3i a, Vector3i b)
        {
            return (a.X == b.X && a.Y == b.Y && a.Z == b.Z);
        }
        public static bool operator !=(Vector3i a, Vector3i b)
        {
            return (a.X != b.X || a.Y != b.Y || a.Z != b.Z);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector3i)obj;
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
        public int CompareTo(Vector3i other)
        {
            if (X != other.X)
                return X < other.X ? -1 : 1;
            else if (Y != other.Y)
                return Y < other.Y ? -1 : 1;
            else if (Z != other.Z)
                return Z < other.Z ? -1 : 1;
            return 0;
        }
        public bool Equals(Vector3i other)
        {
            return (X == other.X && Y == other.Y && Z == other.Z);
        }



        public override string ToString()
        {
            return string.Format("{0} {1} {2}", X, Y, Z);
        }
    }
}
