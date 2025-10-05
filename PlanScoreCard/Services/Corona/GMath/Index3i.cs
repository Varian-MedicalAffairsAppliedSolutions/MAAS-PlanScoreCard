using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.GMath
{
    public struct Index3i : IComparable<Index3i>, IEquatable<Index3i>
    {
        public int a;
        public int b;
        public int c;

        public Index3i(int z) { a = b = c = z; }
        public Index3i(int ii, int jj, int kk) { a = ii; b = jj; c = kk; }
        public Index3i(int[] i2) { a = i2[0]; b = i2[1]; c = i2[2]; }
        public Index3i(Index3i copy) { a = copy.a; b = copy.b; c = copy.b; }

        // reverse last two indices if cycle is true (useful for cw/ccw codes)
        public Index3i(int ii, int jj, int kk, bool cycle)
        {
            a = ii;
            if (cycle) { b = kk; c = jj; }
            else { b = jj; c = kk; }
        }

        static public readonly Index3i Zero = new Index3i(0, 0, 0);
        static public readonly Index3i One = new Index3i(1, 1, 1);
        static public readonly Index3i Max = new Index3i(int.MaxValue, int.MaxValue, int.MaxValue);
        static public readonly Index3i Min = new Index3i(int.MinValue, int.MinValue, int.MinValue);


        public int this[int key]
        {
            get { return (key == 0) ? a : (key == 1) ? b : c; }
            set { if (key == 0) a = value; else if (key == 1) b = value; else c = value; }
        }

        public int[] array
        {
            get { return new int[] { a, b, c }; }
        }


        public int LengthSquared
        {
            get { return a * a + b * b + c * c; }
        }
        public int Length
        {
            get { return (int)Math.Sqrt(LengthSquared); }
        }


        public void Set(Index3i o)
        {
            a = o[0]; b = o[1]; c = o[2];
        }
        public void Set(int ii, int jj, int kk)
        {
            a = ii; b = jj; c = kk;
        }


        public static Index3i operator -(Index3i v)
        {
            return new Index3i(-v.a, -v.b, -v.c);
        }

        public static Index3i operator *(int f, Index3i v)
        {
            return new Index3i(f * v.a, f * v.b, f * v.c);
        }
        public static Index3i operator *(Index3i v, int f)
        {
            return new Index3i(f * v.a, f * v.b, f * v.c);
        }
        public static Index3i operator /(Index3i v, int f)
        {
            return new Index3i(v.a / f, v.b / f, v.c / f);
        }


        public static Index3i operator *(Index3i a, Index3i b)
        {
            return new Index3i(a.a * b.a, a.b * b.b, a.c * b.c);
        }
        public static Index3i operator /(Index3i a, Index3i b)
        {
            return new Index3i(a.a / b.a, a.b / b.b, a.c / b.c);
        }


        public static Index3i operator +(Index3i v0, Index3i v1)
        {
            return new Index3i(v0.a + v1.a, v0.b + v1.b, v0.c + v1.c);
        }
        public static Index3i operator +(Index3i v0, int f)
        {
            return new Index3i(v0.a + f, v0.b + f, v0.c + f);
        }

        public static Index3i operator -(Index3i v0, Index3i v1)
        {
            return new Index3i(v0.a - v1.a, v0.b - v1.b, v0.c - v1.c);
        }
        public static Index3i operator -(Index3i v0, int f)
        {
            return new Index3i(v0.a - f, v0.b - f, v0.c - f);
        }


        public static bool operator ==(Index3i a, Index3i b)
        {
            return (a.a == b.a && a.b == b.b && a.c == b.c);
        }
        public static bool operator !=(Index3i a, Index3i b)
        {
            return (a.a != b.a || a.b != b.b || a.c != b.c);
        }
        public override bool Equals(object obj)
        {
            return this == (Index3i)obj;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ a.GetHashCode();
                hash = (hash * 16777619) ^ b.GetHashCode();
                hash = (hash * 16777619) ^ c.GetHashCode();
                return hash;
            }
        }
        public int CompareTo(Index3i other)
        {
            if (a != other.a)
                return a < other.a ? -1 : 1;
            else if (b != other.b)
                return b < other.b ? -1 : 1;
            else if (c != other.c)
                return c < other.c ? -1 : 1;
            return 0;
        }
        public bool Equals(Index3i other)
        {
            return (a == other.a && b == other.b && c == other.c);
        }


        public override string ToString()
        {
            return string.Format("[{0},{1},{2}]", a, b, c);
        }

    }
}
