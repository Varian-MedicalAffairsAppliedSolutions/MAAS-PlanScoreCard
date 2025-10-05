using CoronaDVH.GMath;
using CoronaDVH.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.Geometry
{
    public abstract class OrientedGridBase<T> : DenseGrid3T<T>
    {
        public OrientedGridBase()
        {
            Dimensions = Vector3i.Zero;
        }


        public OrientedGridBase(int ni, int nj, int nk, T initialValue)
        {
            Resize(ni, nj, nk);
            AssignAllVoxels(initialValue);
        }

        public OrientedGridBase(OrientedGridBase<T> copy)
        {
            Buffer = new T[copy.Buffer.Length];
            Array.Copy(copy.Buffer, Buffer, Buffer.Length);
            Dimensions = copy.Dimensions;
        }

        public U EmptyClone<U>() where U : OrientedGridBase<T>, new()
        {
            var u = new U();
            u.CellSize = CellSize;
            u.Resize(Dimensions.X, Dimensions.Y, Dimensions.Z);
            return u;
        }

        public Vector3f CellSize { get; set; } = Vector3f.One;
        public Frame3f Orientation { get; set; } = Frame3f.Identity;

        /// <summary>
        /// Converts the input real space position to a position in grid coordinates.
        /// Helps find the indices of the point
        /// </summary>
        /// <param name="pt">a point in image coordinates</param>
        /// <returns></returns>
        public Vector3f WorldToGrid(Vector3f pt)
        {
            var oriented = Orientation.ToFrameP(pt);
            return new Vector3f(
               ((oriented.X) / CellSize.X),
               ((oriented.Y) / CellSize.Y),
               ((oriented.Z) / CellSize.Z));
        }

        /// <summary>
        /// Converts the input real space position to a position in grid coordinates.
        /// Helps find the indices of the point
        /// </summary>
        /// <param name="pt">a point in image coordinates</param>
        /// <returns></returns>
        public Vector3i WorldToGridIndex(Vector3f pt)
        {
            var oriented = Orientation.ToFrameP(pt);
            return new Vector3i(
               (int)Math.Floor((oriented.X) / CellSize.X),
               (int)Math.Floor((oriented.Y) / CellSize.Y),
               (int)Math.Floor((oriented.Z) / CellSize.Z));
        }

        /// <summary>
        /// Returns a value at a point in world space
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public T ValueAt(Vector3f pt)
        {
            if (!CanInterpolate)
            {
                var index = this.WorldToGridIndex(pt);
                return this[index];
            }
            else
            {
                var gridPt = WorldToGrid(pt);
                // compute integer coordinates
                int x0 = (int)Math.Floor(gridPt.X);
                int y0 = (int)Math.Floor(gridPt.Y);
                var y1 = y0 + 1;
                int z0 = (int)Math.Floor(gridPt.Z);
                var z1 = z0 + 1;

                // clamp to grid
                if (x0 < 0 || (x0 + 1) >= Dimensions.X ||
                    y0 < 0 || y1 >= Dimensions.Y ||
                    z0 < 0 || z1 >= Dimensions.Z)
                    return Outside;

                // convert double coords to [0,1] range
                float fAx = gridPt.X - x0;
                float fAy = gridPt.Y - y0;
                float fAz = gridPt.Z - z0;
                return (T)(object)Trilinear(x0, y0, z0, fAx, fAy, fAz);
            }
        }

        /// <summary>
        /// Returns a value at a point as a float (used for interpolation). Return NaN if not applicable
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public abstract float FloatValueAt(int x, int y, int z);

        public float FloatValueAt(Vector3i idx)
        {
            return FloatValueAt(idx.X, idx.Y, idx.Z);
        }

        public virtual bool CanInterpolate => true;

        /// <summary>
        /// Takes a 3D cell index and converts it to real world coordinantes
        /// </summary>
        /// <param name="index">index of cell in grid</param>
        /// <returns></returns>
        public Vector3f GridToWorld(Vector3i index)
        {
            if (!CellBounds.Contains(index)) { return new Vector3f(float.NaN); }

            var realX = Orientation.Origin.X + (CellSize.X * (index.X)) * Orientation.X.X +
               (CellSize.Y * (index.Y)) * Orientation.Y.X +
               (CellSize.Z * (index.Z)) * Orientation.Z.X;

            var realY = Orientation.Origin.Y + (CellSize.X * (index.X)) * Orientation.X.Y +
                (CellSize.Y * (index.Y)) * Orientation.Y.Y +
                (CellSize.Z * (index.Z)) * Orientation.Z.Y;

            var realZ = Orientation.Origin.Z + (CellSize.X * (index.X)) * Orientation.X.Z +
                (CellSize.Y * (index.Y)) * Orientation.Y.Z +
                (CellSize.Z * (index.Z)) * Orientation.Z.Z;
            return new Vector3f(realX, realY, realZ);
        }

        /// <summary>
        /// Takes a 3D cell index and converts it to real world coordinantes
        /// </summary>
        /// <param name="index">index of cell in grid</param>
        /// <returns></returns>
        public Vector3f GridToWorld(Vector3f index)
        {
            if (!CellBounds.Contains(new Vector3i((int)Math.Ceiling(index.X), (int)Math.Ceiling(index.Y), (int)Math.Ceiling(index.Z))))
            { return new Vector3f(float.NaN); }

            var realX = Orientation.Origin.X + (CellSize.X * (index.X)) * Orientation.X.X +
               (CellSize.Y * (index.Y)) * Orientation.Y.X +
               (CellSize.Z * (index.Z)) * Orientation.Z.X;

            var realY = Orientation.Origin.Y + (CellSize.X * (index.X)) * Orientation.X.Y +
                (CellSize.Y * (index.Y)) * Orientation.Y.Y +
                (CellSize.Z * (index.Z)) * Orientation.Z.Y;

            var realZ = Orientation.Origin.Z + (CellSize.X * (index.X)) * Orientation.X.Z +
                (CellSize.Y * (index.Y)) * Orientation.Y.Z +
                (CellSize.Z * (index.Z)) * Orientation.Z.Z;
            return new Vector3f(realX, realY, realZ);
        }

        /// <summary>
        /// Trilinearly interpolates from the root voxel to a distance away
        /// </summary>
        /// <param name="nx">root voxel x index</param>
        /// <param name="ny">root voxel y index</param>
        /// <param name="nz">root voxel z index</param>
        /// <param name="dx">distance from center of root voxel in x direction</param>
        /// <param name="dy">distance from center of root voxel in y direction</param>
        /// <param name="dz">distance from center of root voxel in z direction</param>
        /// <returns>the value at dx,dy,dz from the center of the root voxel</returns>
        public float Trilinear(int nx, int ny, int nz, float dx, float dy, float dz)
        {
            return FloatValueAt(nx, ny, nz) * ((1.0f - dx) * (1.0f - dy) * (1.0f - dz))
                         + (FloatValueAt(nx + 1, ny, nz) * (dx * (1.0f - dy) * (1.0f - dz)))
                         + (FloatValueAt(nx, ny + 1, nz) * ((1.0f - dx) * dy * (1.0f - dz)))
                         + (FloatValueAt(nx + 1, ny + 1, nz) * (dx * dy * (1.0f - dz)))
                         + (FloatValueAt(nx, ny, nz + 1) * ((1.0f - dx) * (1.0f - dy) * dz))
                         + (FloatValueAt(nx + 1, ny, nz + 1) * (dx * (1.0f - dy) * dz))
                         + (FloatValueAt(nx, ny + 1, nz + 1) * ((1.0f - dx) * dy * dz))
                         + (FloatValueAt(nx + 1, ny + 1, nz + 1) * (dx * dy * dz));
        }

        /// <summary>
        /// Searches neigbors to see if there is a match, if so, sets the value of current to the set value. Stops searching
        /// after a single match has been found in a neighboring voxel
        /// </summary>
        /// <param name="self">the x,y,z index of current voxel</param>
        /// <param name="directions">directions to search</param>
        /// <param name="match">function to match neigbor</param>
        /// <param name="setValue">value to set current if neighbor match</param>
        /// <returns></returns>
        public bool SearchNeigborsToSet(Vector3i self, SearchDirection[] directions, Func<T, bool> match, T setValue)
        {
            var x = self.X;
            var y = self.Y;
            var z = self.Z;
            bool valueSet = false;

            foreach (var dir in directions)
            {
                switch (dir)
                {
                    case SearchDirection.LEFT: //Left
                        if (match(this[x - 1, y, z]))
                        {
                            this[x, y, z] = setValue;
                            valueSet = true;
                        }
                        break;
                    case SearchDirection.UP: //Up
                        if (match(this[x, y - 1, z]))
                        {
                            this[x, y, z] = setValue;
                            valueSet = true;
                        }
                        break;
                    case SearchDirection.RIGHT: //Right
                        if (match(this[x + 1, y, z]))
                        {
                            this[x, y, z] = setValue;
                            valueSet = true;
                        }
                        break;
                    case SearchDirection.DOWN: //Down
                        if (match(this[x, y + 1, z]))
                        {
                            this[x, y, z] = setValue;
                            valueSet = true;
                        }
                        break;
                }
                if (valueSet) { break; }
            }

            return valueSet;
        }
    }
}
