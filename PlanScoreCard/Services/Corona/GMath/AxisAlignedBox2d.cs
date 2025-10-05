using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.GMath
{
    public struct AxisAlignedBox2d
    {
        public enum ScaleMode
        {
            ScaleRight,
            ScaleLeft,
            ScaleUp,
            ScaleDown,
            ScaleCenter
        }

        public Vector2d Min;

        public Vector2d Max;

        public static readonly AxisAlignedBox2d Empty = new AxisAlignedBox2d(bIgnore: false);

        public static readonly AxisAlignedBox2d Zero = new AxisAlignedBox2d(0.0);

        public static readonly AxisAlignedBox2d UnitPositive = new AxisAlignedBox2d(1.0);

        public static readonly AxisAlignedBox2d Infinite = new AxisAlignedBox2d(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue);

        public double Width => Math.Max(Max.X - Min.X, 0.0);

        public double Height => Math.Max(Max.Y - Min.Y, 0.0);

        public double Area => Width * Height;

        public double DiagonalLength => Math.Sqrt((Max.X - Min.X) * (Max.X - Min.X) + (Max.Y - Min.Y) * (Max.Y - Min.Y));

        public double MaxDim => Math.Max(Width, Height);

        public double MinDim => Math.Min(Width, Height);

        public double MaxUnsignedCoordinate => Math.Max(Math.Max(Math.Abs(Min.X), Math.Abs(Max.X)), Math.Max(Math.Abs(Min.Y), Math.Abs(Max.Y)));

        public Vector2d Diagonal => new Vector2d(Max.X - Min.X, Max.Y - Min.Y);

        public Vector2d Center => new Vector2d(0.5 * (Min.X + Max.X), 0.5 * (Min.Y + Max.Y));

        public AxisAlignedBox2d(bool bIgnore)
        {
            Min = new Vector2d(double.MaxValue, double.MaxValue);
            Max = new Vector2d(double.MinValue, double.MinValue);
        }

        public AxisAlignedBox2d(double xmin, double ymin, double xmax, double ymax)
        {
            Min = new Vector2d(xmin, ymin);
            Max = new Vector2d(xmax, ymax);
        }

        public AxisAlignedBox2d(double fSquareSize)
        {
            Min = new Vector2d(0f, 0f);
            Max = new Vector2d(fSquareSize, fSquareSize);
        }

        public AxisAlignedBox2d(double fWidth, double fHeight)
        {
            Min = new Vector2d(0f, 0f);
            Max = new Vector2d(fWidth, fHeight);
        }

        public AxisAlignedBox2d(Vector2d vMin, Vector2d vMax)
        {
            Min = new Vector2d(Math.Min(vMin.X, vMax.X), Math.Min(vMin.Y, vMax.Y));
            Max = new Vector2d(Math.Max(vMin.X, vMax.X), Math.Max(vMin.Y, vMax.Y));
        }

        public AxisAlignedBox2d(Vector2d vCenter, double fHalfWidth, double fHalfHeight)
        {
            Min = new Vector2d(vCenter.X - fHalfWidth, vCenter.Y - fHalfHeight);
            Max = new Vector2d(vCenter.X + fHalfWidth, vCenter.Y + fHalfHeight);
        }

        public AxisAlignedBox2d(Vector2d vCenter, double fHalfWidth)
        {
            Min = new Vector2d(vCenter.X - fHalfWidth, vCenter.Y - fHalfWidth);
            Max = new Vector2d(vCenter.X + fHalfWidth, vCenter.Y + fHalfWidth);
        }

        public AxisAlignedBox2d(Vector2d vCenter)
        {
            Min = (Max = vCenter);
        }

        public AxisAlignedBox2d(AxisAlignedBox2d o)
        {
            Min = new Vector2d(o.Min);
            Max = new Vector2d(o.Max);
        }

        public Vector2d GetCorner(int i)
        {
            return new Vector2d((i % 3 == 0) ? Min.X : Max.X, (i < 2) ? Min.Y : Max.Y);
        }

        public Vector2d SampleT(double tx, double sy)
        {
            return new Vector2d((1.0 - tx) * Min.X + tx * Max.X, (1.0 - sy) * Min.Y + sy * Max.Y);
        }

        public void Expand(double fRadius)
        {
            Min.X -= fRadius;
            Min.Y -= fRadius;
            Max.X += fRadius;
            Max.Y += fRadius;
        }

        public void Contract(double fRadius)
        {
            Min.X += fRadius;
            Min.Y += fRadius;
            Max.X -= fRadius;
            Max.Y -= fRadius;
        }

        [Obsolete("This method name is confusing. Will remove in future. Use Add() instead")]
        public void Pad(double fPadLeft, double fPadRight, double fPadBottom, double fPadTop)
        {
            Min.X += fPadLeft;
            Min.Y += fPadBottom;
            Max.X += fPadRight;
            Max.Y += fPadTop;
        }

        public void Add(double left, double right, double bottom, double top)
        {
            Min.X += left;
            Min.Y += bottom;
            Max.X += right;
            Max.Y += top;
        }

        public void SetWidth(double fNewWidth, ScaleMode eScaleMode)
        {
            switch (eScaleMode)
            {
                case ScaleMode.ScaleLeft:
                    Min.X = Max.X - fNewWidth;
                    break;
                case ScaleMode.ScaleRight:
                    Max.X = Min.X + fNewWidth;
                    break;
                case ScaleMode.ScaleCenter:
                    {
                        Vector2d center = Center;
                        Min.X = center.X - 0.5 * fNewWidth;
                        Max.X = center.X + 0.5 * fNewWidth;
                        break;
                    }
                default:
                    throw new Exception("Invalid scale mode...");
            }
        }

        public void SetHeight(double fNewHeight, ScaleMode eScaleMode)
        {
            switch (eScaleMode)
            {
                case ScaleMode.ScaleDown:
                    Min.Y = Max.Y - fNewHeight;
                    break;
                case ScaleMode.ScaleUp:
                    Max.Y = Min.Y + fNewHeight;
                    break;
                case ScaleMode.ScaleCenter:
                    {
                        Vector2d center = Center;
                        Min.Y = center.Y - 0.5 * fNewHeight;
                        Max.Y = center.Y + 0.5 * fNewHeight;
                        break;
                    }
                default:
                    throw new Exception("Invalid scale mode...");
            }
        }

        public void Contain(Vector2d v)
        {
            if (v.X < Min.X)
            {
                Min.X = v.X;
            }

            if (v.X > Max.X)
            {
                Max.X = v.X;
            }

            if (v.Y < Min.Y)
            {
                Min.Y = v.Y;
            }

            if (v.Y > Max.Y)
            {
                Max.Y = v.Y;
            }
        }

        public void Contain(ref Vector2d v)
        {
            if (v.X < Min.X)
            {
                Min.X = v.X;
            }

            if (v.X > Max.X)
            {
                Max.X = v.X;
            }

            if (v.Y < Min.Y)
            {
                Min.Y = v.Y;
            }

            if (v.Y > Max.Y)
            {
                Max.Y = v.Y;
            }
        }

        public void Contain(AxisAlignedBox2d box)
        {
            if (box.Min.X < Min.X)
            {
                Min.X = box.Min.X;
            }

            if (box.Max.X > Max.X)
            {
                Max.X = box.Max.X;
            }

            if (box.Min.Y < Min.Y)
            {
                Min.Y = box.Min.Y;
            }

            if (box.Max.Y > Max.Y)
            {
                Max.Y = box.Max.Y;
            }
        }

        public void Contain(ref AxisAlignedBox2d box)
        {
            if (box.Min.X < Min.X)
            {
                Min.X = box.Min.X;
            }

            if (box.Max.X > Max.X)
            {
                Max.X = box.Max.X;
            }

            if (box.Min.Y < Min.Y)
            {
                Min.Y = box.Min.Y;
            }

            if (box.Max.Y > Max.Y)
            {
                Max.Y = box.Max.Y;
            }
        }

        public void Contain(IList<Vector2d> points)
        {
            int count = points.Count;
            if (count <= 0)
            {
                return;
            }

            Vector2d v = points[0];
            Contain(ref v);
            for (int i = 1; i < count; i++)
            {
                v = points[i];
                if (v.X < Min.X)
                {
                    Min.X = v.X;
                }
                else if (v.X > Max.X)
                {
                    Max.X = v.X;
                }

                if (v.Y < Min.Y)
                {
                    Min.Y = v.Y;
                }
                else if (v.Y > Max.Y)
                {
                    Max.Y = v.Y;
                }
            }
        }

        public AxisAlignedBox2d Intersect(AxisAlignedBox2d box)
        {
            AxisAlignedBox2d result = new AxisAlignedBox2d(Math.Max(Min.X, box.Min.X), Math.Max(Min.Y, box.Min.Y), Math.Min(Max.X, box.Max.X), Math.Min(Max.Y, box.Max.Y));
            if (result.Height <= 0.0 || result.Width <= 0.0)
            {
                return Empty;
            }

            return result;
        }

        public bool Contains(Vector2d v)
        {
            return Min.X < v.X && Min.Y < v.Y && Max.X > v.X && Max.Y > v.Y;
        }

        public bool Contains(ref Vector2d v)
        {
            return Min.X < v.X && Min.Y < v.Y && Max.X > v.X && Max.Y > v.Y;
        }

        public bool Contains(AxisAlignedBox2d box2)
        {
            return Contains(ref box2.Min) && Contains(ref box2.Max);
        }

        public bool Contains(ref AxisAlignedBox2d box2)
        {
            return Contains(ref box2.Min) && Contains(ref box2.Max);
        }

        public bool Intersects(AxisAlignedBox2d box)
        {
            return !(box.Max.X < Min.X) && !(box.Min.X > Max.X) && !(box.Max.Y < Min.Y) && !(box.Min.Y > Max.Y);
        }

        public bool Intersects(ref AxisAlignedBox2d box)
        {
            return !(box.Max.X < Min.X) && !(box.Min.X > Max.X) && !(box.Max.Y < Min.Y) && !(box.Min.Y > Max.Y);
        }

        public double Distance(Vector2d v)
        {
            double num = Math.Abs(v.X - Center.X);
            double num2 = Math.Abs(v.Y - Center.Y);
            double num3 = Width * 0.5;
            double num4 = Height * 0.5;
            if (num < num3 && num2 < num4)
            {
                return 0.0;
            }

            if (num > num3 && num2 > num4)
            {
                return Math.Sqrt((num - num3) * (num - num3) + (num2 - num4) * (num2 - num4));
            }

            if (num > num3)
            {
                return num - num3;
            }

            if (num2 > num4)
            {
                return num2 - num4;
            }

            return 0.0;
        }

        public void Translate(Vector2d vTranslate)
        {
            Min.Add(vTranslate);
            Max.Add(vTranslate);
        }

        public void Scale(double scale)
        {
            Min *= scale;
            Max *= scale;
        }

        public void Scale(double scale, Vector2d origin)
        {
            Min = (Min - origin) * scale + origin;
            Max = (Max - origin) * scale + origin;
        }

        public void MoveMin(Vector2d vNewMin)
        {
            Max.X = vNewMin.X + (Max.X - Min.X);
            Max.Y = vNewMin.Y + (Max.Y - Min.Y);
            Min.Set(vNewMin);
        }

        public void MoveMin(double fNewX, double fNewY)
        {
            Max.X = fNewX + (Max.X - Min.X);
            Max.Y = fNewY + (Max.Y - Min.Y);
            Min.Set(fNewX, fNewY);
        }

        public override string ToString()
        {
            return $"[{Min.X:F8},{Max.X:F8}] [{Min.Y:F8},{Max.Y:F8}]";
        }
    }
}
