using ControlzEx.Standard;
using CoronaDVH.Geometry;
using CoronaDVH.GMath;
using Dicom;
using System;
using System.IO;

namespace CoronaDVH.Dicom
{
    public class DicomToGrid
    {
        public static void FillMetadata(OrientedByteGrid grid, DicomDataset dcm)
        {
            var xRes = dcm.GetValues<double>(DicomTag.PixelSpacing)[0];
            var yRes = dcm.GetValues<double>(DicomTag.PixelSpacing)[1];
            grid.CellSize = new Vector3f(xRes, yRes, grid.CellSize.Z);

            //Origin
            var originRaw = dcm.GetValues<double>(DicomTag.ImagePositionPatient);
            var origin = new Vector3f(originRaw[0], originRaw[1], originRaw[2]);

            var orientRaw = dcm.GetValues<double>(DicomTag.ImageOrientationPatient);
            var xDir = new Vector3f(orientRaw[0], orientRaw[1], orientRaw[2]);
            var yDir = new Vector3f(orientRaw[3], orientRaw[4], orientRaw[5]);
            var zDir = xDir.Cross(yDir);
            grid.Orientation = new Frame3f(origin, xDir, yDir, zDir);

            var xDim = dcm.GetValues<ushort>(DicomTag.Columns)[0];
            var yDim = dcm.GetValues<ushort>(DicomTag.Rows)[0];
            grid.Dimensions = new Vector3i(xDim, yDim, grid.Dimensions.Z);

            grid.BitsAllocated = dcm.GetValues<ushort>(DicomTag.BitsAllocated)[0];
            grid.BitsStored = dcm.GetValues<ushort>(DicomTag.BitsStored)[0];
        }


        public static Func<BinaryReader, float> GetValueConverter(OrientedByteGrid grid)
        {
            Func<BinaryReader, float> valueConverter = null;
            switch (grid.BytesAllocated)
            {
                case 1: valueConverter = (br) => (int)br.ReadByte(); break;
                case 2: valueConverter = (br) => br.ReadInt16(); break;
                case 4: valueConverter = (br) => br.ReadInt32(); break;
                case 8: valueConverter = (br) => br.ReadInt64(); break;
                default: throw new NotSupportedException();
            }
            return valueConverter;
        }
    }
}
