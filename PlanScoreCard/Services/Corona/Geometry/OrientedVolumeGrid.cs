using CoronaDVH.GMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.Geometry
{
    /// <summary>
    /// A grid that represents some kind of volume object. It is like a labled voxel grid, but supports more
    /// than inside and outside labels.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrientedVolumeGrid<T> : OrientedGridBase<T>
    {
        public OrientedVolumeGrid() : base()
        {
        }

        public OrientedVolumeGrid(int x, int y, int z, T initialValue) : base(x, y, z, initialValue)
        {
        }

        public OrientedVolumeGrid(Vector3i dim, T initialValue) : base(dim.X, dim.Y, dim.Z, initialValue)
        {
        }

        public AxisAlignedBox3f VolumeBounds { get; set; } = AxisAlignedBox3f.Zero;

        public override float FloatValueAt(int x, int y, int z)
        {
            return (float)(object)this[x, y, z];
        }

        /// <summary>
        /// Returns a NxN block of voxels centered at the input position (used in chamfering)
        /// </summary>
        /// <param name="cx">center x</param>
        /// <param name="cy">center y</param>
        /// <param name="cz">center z</param>
        /// <param name="size">the size N of the block</param>
        /// <returns>an NxN block of voxels centered at the input position</returns>
        public float[] GetFloatBlock(int cx, int cy, int cz, int size)
        {
            var block = new float[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var sampleX = cx - 1 + x;
                    var sampleY = cy - 1 + y;
                    if (sampleX < 0 || sampleX > Dimensions.X - 1 || sampleY < 0 || sampleY > Dimensions.Y - 1)
                    {
                        block[IndexUtil.ToGrid2Linear(x, y, size)] = (float)(object)this.Outside;
                    }
                    else
                    {
                        block[IndexUtil.ToGrid2Linear(x, y, size)] = FloatValueAt(sampleX, sampleY, cz);
                    }
                }
            }
            return block;
        }
    }
}
