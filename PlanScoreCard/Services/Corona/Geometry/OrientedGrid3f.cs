using CoronaDVH.GMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.Geometry
{
    public class OrientedGrid3f : OrientedGridBase<float>
    {
        public OrientedGrid3f() : base()
        {
        }

        public OrientedGrid3f(DenseGrid3f copy)
        {
            Buffer = new float[copy.Buffer.Length];
            Array.Copy(copy.Buffer, Buffer, Buffer.Length);
            Dimensions = copy.Dimensions;
        }

        public OrientedGrid3f(OrientedGrid3f copy) : base(copy)
        {
        }

        public OrientedGrid3f(Vector3i dimensions, float initialValue) : this(dimensions.X, dimensions.Y, dimensions.Z, initialValue)
        {
        }

        public OrientedGrid3f(int ni, int nj, int nk, float initialValue) : base(ni, nj, nk, initialValue)
        {
        }

        public new OrientedGrid3f EmptyClone()
        {
            var clone = new OrientedGrid3f(Dimensions.X, Dimensions.Y, Dimensions.Z, default)
            {
                Orientation = new Frame3f(Orientation),
                CellSize = new Vector3f(CellSize)
            };
            return clone;
        }

        public OrientedGrid3f ResampleOn(OrientedGrid3f toGrid)
        {
            var resampled = toGrid.EmptyClone();
            var indices = resampled.Indices().ToList();
            Parallel.ForEach(indices, idx =>
            {
                var pt = toGrid.GridToWorld(idx);
                resampled[idx] = this.ValueAt(pt);
            });
            return resampled;
        }

        public override float FloatValueAt(int x, int y, int z)
        {
            return this[x, y, z];
        }
    }
}
