using CoronaDVH.GMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.Geometry
{
    /// <summary>
    /// 3D dense grid of floating-point scalar values. 
    /// </summary>
    public class DenseGrid3f : DenseGrid3T<float>
    {
        public DenseGrid3f()
        {
            Dimensions = Vector3i.Zero;
        }


        public DenseGrid3f(int ni, int nj, int nk, float initialValue)
        {
            Resize(ni, nj, nk);
            AssignAllVoxels(initialValue);
        }

        public DenseGrid3f(DenseGrid3f copy)
        {
            Buffer = new float[copy.Buffer.Length];
            Array.Copy(copy.Buffer, Buffer, Buffer.Length);
            Dimensions = copy.Dimensions;
        }

        public new DenseGrid3f EmptyClone()
        {
            return new DenseGrid3f(Dimensions.X, Dimensions.Y, Dimensions.Z, default);
        }


        /// <summary>
        /// Forces value at location to be less than or equal the maximum value passed in (truncates to input value if over)
        /// </summary>
        /// <param name="ijk">index of sampled voxel</param>
        /// <param name="maxValue">maximum value allowed</param>
        public void SetUpperThreshold(ref Vector3i ijk, float maxValue)
        {
            int idx = ijk.X + Dimensions.X * (ijk.Y + Dimensions.Y * ijk.Z);
            if (maxValue < Buffer[idx])
                Buffer[idx] = maxValue;
        }

        /// <summary>
        /// Forces value at location to be greater than or equal the min value passed in (truncates to input value if under)
        /// </summary>
        /// <param name="ijk">index of sampled voxel</param>
        /// <param name="minValue">maximum value allowed</param>
        public void SetLowerThreshold(ref Vector3i ijk, float minValue)
        {
            int idx = ijk.X + Dimensions.X * (ijk.Y + Dimensions.Y * ijk.Z);
            if (minValue > Buffer[idx])
                Buffer[idx] = minValue;
        }


        public IEnumerable<Vector3i> InsetIndices(int border_width)
        {
            int stopy = Dimensions.Y - border_width, stopx = Dimensions.X - border_width;
            for (int z = border_width; z < Dimensions.Z - border_width; ++z)
            {
                for (int y = border_width; y < stopy; ++y)
                {
                    for (int x = border_width; x < stopx; ++x)
                        yield return new Vector3i(x, y, z);
                }
            }
        }

        public static DenseGrid3f operator +(DenseGrid3f dg1, DenseGrid3f dg2)
        {
            DenseGrid3f.CheckDimensions(dg1, dg2);
            var sum = dg1.EmptyClone();
            Parallel.For(0, sum.Buffer.Length, (Action<int>)(i =>
            {
                sum.Buffer[i] = dg1.Buffer[i] + dg2.Buffer[i];
            }));
            return sum;
        }

        public static DenseGrid3f operator -(DenseGrid3f dg1, DenseGrid3f dg2)
        {
            DenseGrid3f.CheckDimensions(dg1, dg2);
            var diff = dg1.EmptyClone();
            Parallel.For(0, diff.Buffer.Length, (Action<int>)(i =>
            {
                diff.Buffer[i] = dg1.Buffer[i] - dg2.Buffer[i];
            }));
            return diff;
        }

        public static DenseGrid3f operator /(DenseGrid3f dg1, DenseGrid3f dg2)
        {
            CheckDimensions(dg1, dg2);
            var quotient = dg1.EmptyClone();
            Parallel.For(0, quotient.Buffer.Length, (Action<int>)(i =>
            {
                quotient.Buffer[i] = dg1.Buffer[i] / dg2.Buffer[i];
            }));
            return quotient;
        }

        public static DenseGrid3f operator *(DenseGrid3f dg1, DenseGrid3f dg2)
        {
            CheckDimensions(dg1, dg2);
            var product = dg1.EmptyClone();
            Parallel.For(0, product.Buffer.Length, (Action<int>)(i =>
            {
                product.Buffer[i] = dg1.Buffer[i] * dg2.Buffer[i];
            }));
            return product;
        }

        public static DenseGrid3f operator *(DenseGrid3f dg1, float scale)
        {
            var product = dg1.EmptyClone();
            Parallel.For(0, product.Buffer.Length, (Action<int>)(i =>
            {
                product.Buffer[i] = dg1.Buffer[i] * scale;
            }));
            return product;
        }
    }
}
