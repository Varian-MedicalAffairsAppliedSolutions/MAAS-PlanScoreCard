using CoronaDVH.GMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.Geometry
{
    public class DenseGrid3T<TSelf>
    {
        public TSelf[] Buffer;
        private Vector3i dimensions;
        public Vector3i Dimensions
        {
            get { return dimensions; }
            set { dimensions = value; }
        }

        public DenseGrid3T()
        {
            Dimensions = Vector3i.Zero;
        }

        public DenseGrid3T(DenseGrid3T<TSelf> copy)
        {
            Buffer = new TSelf[copy.Buffer.Length];
            Array.Copy(copy.Buffer, Buffer, Buffer.Length);
            Dimensions = copy.Dimensions;
        }

        public DenseGrid3T(int ni, int nj, int nk, TSelf initialValue)
        {
            Resize(ni, nj, nk);
            AssignAllVoxels(initialValue);
        }

        public AxisAlignedBox3i CellBounds
        {
            get { return new AxisAlignedBox3i(0, 0, 0, Dimensions.X, Dimensions.Y, Dimensions.Z); }
        }
        public AxisAlignedBox3i CellBoundsInclusive
        {
            get { return new AxisAlignedBox3i(0, 0, 0, Dimensions.X - 1, Dimensions.Y - 1, Dimensions.Z - 1); }
        }


        public virtual DenseGrid3T<TSelf> EmptyClone()
        {
            return new DenseGrid3T<TSelf>(Dimensions.X, Dimensions.Y, Dimensions.Z, default);
        }


        /// <summary>
        /// Resizes and resets the voxel matrix to the input dimensions
        /// </summary>
        /// <param name="ni">x dim</param>
        /// <param name="nj">y dim</param>
        /// <param name="nk">z dim</param>
        public void Resize(int ni, int nj, int nk)
        {
            Buffer = new TSelf[ni * nj * nk];
            this.dimensions.X = ni; this.dimensions.Y = nj; this.dimensions.Z = nk;
        }

        /// <summary>
        /// Resizes and resets the voxel matrix to the input dimensions
        /// </summary>
        /// <param name="ni">x dim</param>
        /// <param name="nj">y dim</param>
        /// <param name="nk">z dim</param>
        public void Resize(Vector3i dimensions)
        {
            Resize(dimensions.X, dimensions.Y, dimensions.Z);
        }

        public void Resize()
        {
            Buffer = new TSelf[Dimensions.X * Dimensions.Y * Dimensions.Z];
        }

        public void AssignAllVoxels(TSelf value)
        {
            for (int i = 0; i < Buffer.Length; ++i)
                Buffer[i] = value;
        }

        public TSelf this[int i]
        {
            get { return Buffer[i]; }
            set { Buffer[i] = value; }
        }

        public virtual TSelf Outside { get; set; } = default;

        public TSelf this[int i, int j, int k]
        {
            get { return Buffer[i + Dimensions.X * (j + Dimensions.Y * k)]; }
            set { Buffer[i + Dimensions.X * (j + Dimensions.Y * k)] = value; }
        }

        public TSelf this[Vector3i ijk]
        {
            get { return Buffer[ijk.X + Dimensions.X * (ijk.Y + Dimensions.Y * ijk.Z)]; }
            set { Buffer[ijk.X + Dimensions.X * (ijk.Y + Dimensions.Y * ijk.Z)] = value; }
        }

        /*public void VoxelWiseApply(Func<TSelf, TSelf> f)
        {
            Span<TSelf> elements = Buffer;
            ref var zeroPointer = ref MemoryMarshal.GetReference(elements);

            for (int i = 0; i < Buffer.Length; i++)
            {
                ref var current = ref Unsafe.Add(ref zeroPointer, i);
                current = f(current);
            }
        }
        //array instead until System.Memory is availab.e 


        public void VoxelWiseApply(Func<Vector3i, TSelf> f)
        {
            Span<TSelf> elements = Buffer;
            ref var zeroPointer = ref MemoryMarshal.GetReference(elements);

            for (int k = 0; k < Dimensions.Z; k++)
            {
                for (int j = 0; j < Dimensions.Y; j++)
                {
                    for (int i = 0; i < Dimensions.X; i++)
                    {
                        int idx = i + Dimensions.X * (j + Dimensions.Y * k);
                        ref var current = ref Unsafe.Add(ref zeroPointer, idx);
                        current = f(new Vector3i(i, j, k));
                    }
                }
            }
        }

        public void ParallelForEachVoxel(Action<Vector3i> voxelAction)
        {
            Parallel.For(0, Dimensions.Z, z =>
            {
                Span<TSelf> elements = Buffer;
                ref var zeroPointer = ref MemoryMarshal.GetReference(elements);

                for (int y = 0; y < Dimensions.Y; y++)
                {
                    for (int x = 0; x < Dimensions.X; x++)
                    {
                        int idx = x + Dimensions.X * (y + Dimensions.Y * z);
                        ref var current = ref Unsafe.Add(ref zeroPointer, idx);
                        voxelAction(new Vector3i(x, y, z));
                    }
                }
            });
        }*/
        public void VoxelWiseApply(Func<TSelf, TSelf> f)
        {
            for (int i = 0; i < Buffer.Length; i++)
            {
                Buffer[i] = f(Buffer[i]);
            }
        }

        public void VoxelWiseApply(Func<Vector3i, TSelf> f)
        {
            for (int k = 0; k < Dimensions.Z; k++)
            {
                for (int j = 0; j < Dimensions.Y; j++)
                {
                    for (int i = 0; i < Dimensions.X; i++)
                    {
                        int idx = i + Dimensions.X * (j + Dimensions.Y * k);
                        Buffer[idx] = f(new Vector3i(i, j, k));
                    }
                }
            }
        }

        public void ParallelForEachVoxel(Action<Vector3i> voxelAction)
        {
            Parallel.For(0, Dimensions.Z, z =>
            {
                for (int y = 0; y < Dimensions.Y; y++)
                {
                    for (int x = 0; x < Dimensions.X; x++)
                    {
                        voxelAction(new Vector3i(x, y, z));
                    }
                }
            });
        }


        public IEnumerable<Vector3i> Indices()
        {
            for (int z = 0; z < Dimensions.Z; ++z)
            {
                for (int y = 0; y < Dimensions.Y; ++y)
                {
                    for (int x = 0; x < Dimensions.X; ++x)
                        yield return new Vector3i(x, y, z);
                }
            }
        }

        protected static void CheckDimensions(DenseGrid3T<TSelf> dg1, DenseGrid3T<TSelf> dg2)
        {
            if (dg1.Dimensions != dg2.Dimensions)
            {
                throw new Exception("Grid dimensions do not match");
            }
        }

        public Vector3i ToIndex(int idx)
        {
            int x = idx % Dimensions.X;
            int y = (idx / Dimensions.X) % Dimensions.Y;
            int z = idx / (Dimensions.X * Dimensions.Y);
            return new Vector3i(x, y, z);
        }
        public int ToLinear(int i, int j, int k)
        {
            return i + Dimensions.X * (j + Dimensions.Y * k);
        }
        public int ToLinear(ref Vector3i ijk)
        {
            return ijk.X + Dimensions.X * (ijk.Y + Dimensions.Y * ijk.Z);
        }
        public int ToLinear(Vector3i ijk)
        {
            return ijk.X + Dimensions.X * (ijk.Y + Dimensions.Y * ijk.Z);
        }
    }
}
