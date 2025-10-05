using CoronaDVH.Dicom;
using CoronaDVH.GMath;
using Dicom;
using Dicom.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoronaDVH.Geometry
{
    public class ImageGrid : OrientedByteGrid
    {
        public static ImageGrid FromFiles(string[] slices)
        {
            var seriesInfo = PreScanSeries(slices);
            var grid = new ImageGrid();
            grid.Resize(seriesInfo.XSize, seriesInfo.YSize, seriesInfo.ZSize);
            var originImage = seriesInfo.SliceMap[seriesInfo.Z0];
            var remaining = seriesInfo.SliceMap.Values.Where(v => v != originImage).ToList();

            slices = new string[] { originImage }.Concat(remaining).ToArray();

            for (int z = 0; z < slices.Length; z++)
            {
                var slicePath = slices[z];
                var dcm = DicomFile.Open(slicePath);

                if (z == 0)
                {
                    DicomToGrid.FillMetadata(grid, dcm.Dataset);
                    var pixelSpacing = dcm.Dataset.GetValues<float>(DicomTag.PixelSpacing);
                    var cellZ = dcm.Dataset.GetValue<float>(DicomTag.SliceThickness, 0);
                    grid.CellSize = new Vector3f(pixelSpacing[0], pixelSpacing[1], cellZ);
                }

                var slice = dcm.Dataset.GetValues<decimal>(DicomTag.ImagePositionPatient);
                var sliceN = (int)(((double)slice[2] - seriesInfo.Z0) / seriesInfo.SliceThickness);

                ////FILL VOXELS
                Func<BinaryReader, float> valueConverter = DicomToGrid.GetValueConverter(grid);
                var values = new List<float>();
                var pixelData = DicomPixelData.Create(dcm.Dataset);
                for (int j = 0; j < pixelData.NumberOfFrames; j++)
                {
                    var intercept = dcm.Dataset.GetValue<float>(DicomTag.RescaleIntercept, 0);
                    var slope = dcm.Dataset.GetValue<float>(DicomTag.RescaleSlope, 0);

                    var voxels = pixelData.GetFrame(j).Data;
                    var k = sliceN * seriesInfo.XSize * seriesInfo.YSize;
                    using (BinaryReader br = new BinaryReader(new MemoryStream(voxels)))
                    {
                        while (br.BaseStream.Position < br.BaseStream.Length)
                            grid[k++] = (float)(valueConverter(br)) * slope + intercept;
                    }
                }
            }
            return grid;
        }

        private static SeriesInfo PreScanSeries(string[] slices)
        {
            var maxZ = double.MinValue;
            var minZ = double.MaxValue;

            var si = new SeriesInfo();
            for (int i = 0; i < slices.Length; i++)
            {
                var slicePath = slices[i];
                var dcm = DicomFile.Open(slicePath);
                var series = dcm.Dataset.GetValue<string>(DicomTag.SeriesInstanceUID, 0);
                if (i == 0)
                {
                    si.SeriesUID = series;
                    si.SliceThickness = dcm.Dataset.GetValue<double>(DicomTag.SliceThickness, 0);
                    si.XSize = dcm.Dataset.GetValue<int>(DicomTag.Columns, 0);
                    si.YSize = dcm.Dataset.GetValue<int>(DicomTag.Rows, 0);
                    var orientRaw = dcm.Dataset.GetValues<double>(DicomTag.ImageOrientationPatient);
                    var xDir = new Vector3f(orientRaw[0], orientRaw[1], orientRaw[2]);
                    var yDir = new Vector3f(orientRaw[3], orientRaw[4], orientRaw[5]);
                    si.ZDir = xDir.Cross(yDir);
                }
                if (si.SeriesUID == series)
                {
                    var instanceNum = dcm.Dataset.GetValue<int>(DicomTag.InstanceNumber, 0);
                    var slice = dcm.Dataset.GetValues<double>(DicomTag.ImagePositionPatient);
                    var z = slice[2];
                    maxZ = Math.Max(z, maxZ);
                    minZ = Math.Min(z, minZ);

                    if (!si.SliceMap.ContainsKey(z))
                    {
                        si.SliceMap.Add(z, slices[i]);
                    }
                }
            }

            si.ZSize = (int)((maxZ - minZ) / si.SliceThickness) + 1;
            si.Z0 = si.ZDir.Z > 0 ? minZ : maxZ;
            return si;
        }
    }
}
