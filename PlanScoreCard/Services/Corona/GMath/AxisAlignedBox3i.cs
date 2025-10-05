using System;
using System.Collections.Generic;

namespace CoronaDVH.GMath
{
    public struct AxisAlignedBox3i : IComparable<AxisAlignedBox3i>, IEquatable<AxisAlignedBox3i>
    {
        public Vector3i Min;
        public Vector3i Max;

        public static readonly AxisAlignedBox3i Empty = new AxisAlignedBox3i(false);
        public static readonly AxisAlignedBox3i Zero = new AxisAlignedBox3i(0);
        public static readonly AxisAlignedBox3i UnitPositive = new AxisAlignedBox3i(1);
        public static readonly AxisAlignedBox3i Infinite =
            new AxisAlignedBox3i(int.MinValue, int.MinValue, int.MinValue, int.MaxValue, int.MaxValue, int.MaxValue);


        public AxisAlignedBox3i(bool bIgnore)
        {
            Min = new Vector3i(int.MaxValue, int.MaxValue, int.MaxValue);
            Max = new Vector3i(int.MinValue, int.MinValue, int.MinValue);
        }

        public AxisAlignedBox3i(int xmin, int ymin, int zmin, int xmax, int ymax, int zmax)
        {
            Min = new Vector3i(xmin, ymin, zmin);
            Max = new Vector3i(xmax, ymax, zmax);
        }

        public AxisAlignedBox3i(int fCubeSize)
        {
            Min = new Vector3i(0, 0, 0);
            Max = new Vector3i(fCubeSize, fCubeSize, fCubeSize);
        }

        public AxisAlignedBox3i(int fWidth, int fHeight, int fDepth)
        {
            Min = new Vector3i(0, 0, 0);
            Max = new Vector3i(fWidth, fHeight, fDepth);
        }

        public AxisAlignedBox3i(Vector3i vMin, Vector3i vMax)
        {
            Min = new Vector3i(Math.Min(vMin.X, vMax.X), Math.Min(vMin.Y, vMax.Y), Math.Min(vMin.Z, vMax.Z));
            Max = new Vector3i(Math.Max(vMin.X, vMax.X), Math.Max(vMin.Y, vMax.Y), Math.Max(vMin.Z, vMax.Z));
        }

        public AxisAlignedBox3i(Vector3i vCenter, int fHalfWidth, int fHalfHeight, int fHalfDepth)
        {
            Min = new Vector3i(vCenter.X - fHalfWidth, vCenter.Y - fHalfHeight, vCenter.Z - fHalfDepth);
            Max = new Vector3i(vCenter.X + fHalfWidth, vCenter.Y + fHalfHeight, vCenter.Z + fHalfDepth);
        }
        public AxisAlignedBox3i(Vector3i vCenter, int fHalfSize)
        {
            Min = new Vector3i(vCenter.X - fHalfSize, vCenter.Y - fHalfSize, vCenter.Z - fHalfSize);
            Max = new Vector3i(vCenter.X + fHalfSize, vCenter.Y + fHalfSize, vCenter.Z + fHalfSize);
        }

        public AxisAlignedBox3i(Vector3i vCenter)
        {
            Min = Max = vCenter;
        }

        public int Width
        {
            get { return Math.Max(Max.X - Min.X, 0); }
        }
        public int Height
        {
            get { return Math.Max(Max.Y - Min.Y, 0); }
        }
        public int Depth
        {
            get { return Math.Max(Max.Z - Min.Z, 0); }
        }

        public int Volume
        {
            get { return Width * Height * Depth; }
        }
        public int DiagonalLength
        {
            get
            {
                return (int)Math.Sqrt((Max.X - Min.X) * (Max.X - Min.X)
                    + (Max.Y - Min.Y) * (Max.Y - Min.Y) + (Max.Z - Min.Z) * (Max.Z - Min.Z));
            }
        }
        public int MaxDim
        {
            get { return Math.Max(Width, Math.Max(Height, Depth)); }
        }

        public Vector3i Diagonal
        {
            get { return new Vector3i(Max.X - Min.X, Max.Y - Min.Y, Max.Z - Min.Z); }
        }
        public Vector3i Extents
        {
            get { return new Vector3i((Max.X - Min.X) / 2, (Max.Y - Min.Y) / 2, (Max.Z - Min.Z) / 2); }
        }
        public Vector3i Center
        {
            get { return new Vector3i((Min.X + Max.X) / 2, (Min.Y + Max.Y) / 2, (Min.Z + Max.Z) / 2); }
        }


        public static bool operator ==(AxisAlignedBox3i a, AxisAlignedBox3i b)
        {
            return a.Min == b.Min && a.Max == b.Max;
        }
        public static bool operator !=(AxisAlignedBox3i a, AxisAlignedBox3i b)
        {
            return a.Min != b.Min || a.Max != b.Max;
        }
        public override bool Equals(object obj)
        {
            return this == (AxisAlignedBox3i)obj;
        }
        public bool Equals(AxisAlignedBox3i other)
        {
            return this == other;
        }
        public int CompareTo(AxisAlignedBox3i other)
        {
            int c = this.Min.CompareTo(other.Min);
            if (c == 0)
                return this.Max.CompareTo(other.Max);
            return c;
        }
        public override int GetHashCode()
        {
            unchecked
            { // Overflow is fine, just wrap
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ Min.GetHashCode();
                hash = (hash * 16777619) ^ Max.GetHashCode();
                return hash;
            }
        }


        // TODO
        ////! 0 == bottom-left, 1 = bottom-right, 2 == top-right, 3 == top-left
        //public Vector3i GetCorner(int i) {
        //    return new Vector3i((i % 3 == 0) ? Min.X : Max.X, (i < 2) ? Min.Y : Max.Y);
        //}

        //! value is subtracted from min and added to max
        public void Expand(int nRadius)
        {
            Min.X -= nRadius; Min.Y -= nRadius; Min.Z -= nRadius;
            Max.X += nRadius; Max.Y += nRadius; Max.Z += nRadius;
        }
        //! value is added to min and subtracted from max
        public void Contract(int nRadius)
        {
            Min.X += nRadius; Min.Y += nRadius; Min.Z += nRadius;
            Max.X -= nRadius; Max.Y -= nRadius; Max.Z -= nRadius;
        }

        public void Scale(int sx, int sy, int sz)
        {
            Vector3i c = Center;
            Vector3i e = Extents; e.X *= sx; e.Y *= sy; e.Z *= sz;
            Min = new Vector3i(c.X - e.X, c.Y - e.Y, c.Z - e.Z);
            Max = new Vector3i(c.X + e.X, c.Y + e.Y, c.Z + e.Z);
        }

        public void Contain(Vector3i v)
        {
            Min.X = Math.Min(Min.X, v.X);
            Min.Y = Math.Min(Min.Y, v.Y);
            Min.Z = Math.Min(Min.Z, v.Z);
            Max.X = Math.Max(Max.X, v.X);
            Max.Y = Math.Max(Max.Y, v.Y);
            Max.Z = Math.Max(Max.Z, v.Z);
        }

        public void Contain(AxisAlignedBox3i box)
        {
            Min.X = Math.Min(Min.X, box.Min.X);
            Min.Y = Math.Min(Min.Y, box.Min.Y);
            Min.Z = Math.Min(Min.Z, box.Min.Z);
            Max.X = Math.Max(Max.X, box.Max.X);
            Max.Y = Math.Max(Max.Y, box.Max.Y);
            Max.Z = Math.Max(Max.Z, box.Max.Z);
        }


        public void Contain(Vector3f v)
        {
            Min.X = Math.Min(Min.X, (int)v.X);
            Min.Y = Math.Min(Min.Y, (int)v.Y);
            Min.Z = Math.Min(Min.Z, (int)v.Z);
            Max.X = Math.Max(Max.X, (int)v.X);
            Max.Y = Math.Max(Max.Y, (int)v.Y);
            Max.Z = Math.Max(Max.Z, (int)v.Z);
        }

        public AxisAlignedBox3i Intersect(AxisAlignedBox3i box)
        {
            AxisAlignedBox3i intersect = new AxisAlignedBox3i(
                Math.Max(Min.X, box.Min.X), Math.Max(Min.Y, box.Min.Y), Math.Max(Min.Z, box.Min.Z),
                Math.Min(Max.X, box.Max.X), Math.Min(Max.Y, box.Max.Y), Math.Min(Max.Z, box.Max.Z));
            if (intersect.Height <= 0 || intersect.Width <= 0 || intersect.Depth <= 0)
                return AxisAlignedBox3i.Empty;
            else
                return intersect;
        }

        public bool Contains(Vector3i v)
        {
            return (Min.X <= v.X) && (Min.Y <= v.Y) && (Min.Z <= v.Z)
                && (Max.X >= v.X) && (Max.Y >= v.Y) && (Max.Z >= v.Z);
        }
        public bool Intersects(AxisAlignedBox3i box)
        {
            return !((box.Max.X <= Min.X) || (box.Min.X >= Max.X)
                || (box.Max.Y <= Min.Y) || (box.Min.Y >= Max.Y)
                || (box.Max.Z <= Min.Z) || (box.Min.Z >= Max.Z));
        }


        public double DistanceSquared(Vector3i v)
        {
            int dx = (v.X < Min.X) ? Min.X - v.X : (v.X > Max.X ? v.X - Max.X : 0);
            int dy = (v.Y < Min.Y) ? Min.Y - v.Y : (v.Y > Max.Y ? v.Y - Max.Y : 0);
            int dz = (v.Z < Min.Z) ? Min.Z - v.Z : (v.Z > Max.Z ? v.Z - Max.Z : 0);
            return dx * dx + dy * dy + dz * dz;
        }
        public int Distance(Vector3i v)
        {
            return (int)Math.Sqrt(DistanceSquared(v));
        }


        public Vector3i NearestPoint(Vector3i v)
        {
            int x = (v.X < Min.X) ? Min.X : (v.X > Max.X ? Max.X : v.X);
            int y = (v.Y < Min.Y) ? Min.Y : (v.Y > Max.Y ? Max.Y : v.Y);
            int z = (v.Z < Min.Z) ? Min.Z : (v.Z > Max.Z ? Max.Z : v.Z);
            return new Vector3i(x, y, z);
        }


        /// <summary>
        /// Clamp v to grid bounds [min, max]
        /// </summary>
        public Vector3i ClampInclusive(Vector3i v)
        {
            return new Vector3i(
                MathUtil.Clamp(v.X, Min.X, Max.X),
                MathUtil.Clamp(v.Y, Min.Y, Max.Y),
                MathUtil.Clamp(v.Z, Min.Z, Max.Z));
        }

        /// <summary>
        /// clamp v to grid bounds [min,max)
        /// </summary>
        public Vector3i ClampExclusive(Vector3i v)
        {
            return new Vector3i(
                MathUtil.Clamp(v.X, Min.X, Max.X - 1),
                MathUtil.Clamp(v.Y, Min.Y, Max.Y - 1),
                MathUtil.Clamp(v.Z, Min.Z, Max.Z - 1));
        }



        //! relative translation
        public void Translate(Vector3i vTranslate)
        {
            Min.Add(vTranslate);
            Max.Add(vTranslate);
        }

        public void MoveMin(Vector3i vNewMin)
        {
            Max.X = vNewMin.X + (Max.X - Min.X);
            Max.Y = vNewMin.Y + (Max.Y - Min.Y);
            Max.Z = vNewMin.Z + (Max.Z - Min.Z);
            Min.Set(vNewMin);
        }
        public void MoveMin(int fNewX, int fNewY, int fNewZ)
        {
            Max.X = fNewX + (Max.X - Min.X);
            Max.Y = fNewY + (Max.Y - Min.Y);
            Max.Z = fNewZ + (Max.Z - Min.Z);
            Min.Set(fNewX, fNewY, fNewZ);
        }




        public IEnumerable<Vector3i> IndicesInclusive()
        {
            for (int zi = Min.Z; zi <= Max.Z; ++zi)
            {
                for (int yi = Min.Y; yi <= Max.Y; ++yi)
                {
                    for (int xi = Min.X; xi <= Max.X; ++xi)
                        yield return new Vector3i(xi, yi, zi);
                }
            }
        }
        public IEnumerable<Vector3i> IndicesExclusive()
        {
            for (int zi = Min.Z; zi < Max.Z; ++zi)
            {
                for (int yi = Min.Y; yi < Max.Y; ++yi)
                {
                    for (int xi = Min.X; xi < Max.X; ++xi)
                        yield return new Vector3i(xi, yi, zi);
                }
            }
        }



        public override string ToString()
        {
            return string.Format("x[{0},{1}] y[{2},{3}] z[{4},{5}]", Min.X, Max.X, Min.Y, Max.Y, Min.Z, Max.Z);
        }
    }
}
