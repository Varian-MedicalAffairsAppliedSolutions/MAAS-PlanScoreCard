using CoronaDVH.Geometry;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CoronaDVH.Helpers
{
    public class GridIO
    {
        public class StructConversion
        {
            public static byte[] GetBytes<T>(T str) where T : struct
            {
                int size = Marshal.SizeOf(str);
                byte[] arr = new byte[size];

                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(str, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);
                return arr;
            }

            public static T FromBytes<T>(byte[] arr) where T : struct
            {
                T str = new T();

                int size = Marshal.SizeOf(str);
                IntPtr ptr = Marshal.AllocHGlobal(size);

                Marshal.Copy(arr, 0, ptr, size);

                str = (T)Marshal.PtrToStructure(ptr, str.GetType());
                Marshal.FreeHGlobal(ptr);

                return str;
            }
        }
        public static void Save(OrientedGrid3f grid, string savePath)
        {
            using (FileStream file = File.Create(savePath))
            {
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    writer.Write(StructConversion.GetBytes(grid.Dimensions));
                    writer.Write(StructConversion.GetBytes(grid.CellSize));
                    writer.Write(StructConversion.GetBytes(grid.Orientation));
                    foreach (var vox in grid.Buffer)
                    {
                        writer.Write(vox);
                    }
                }
            }
        }
    }
}
