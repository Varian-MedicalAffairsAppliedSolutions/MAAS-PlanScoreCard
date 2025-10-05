using CoronaDVH.GMath;
using Dicom;
using Dicom.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoronaDVH.Dicom
{
    public class DoseToGrid : DicomToGrid
    {
        public static DoseGrid FromFile(string dcmFile, double? prescribedDoseGy = null, bool convertToRelative = true)
        {
            var dcm = DicomFile.Open(dcmFile);

            var matrix = new DoseGrid();
            matrix.PrescriptionDoseGy = prescribedDoseGy;

            //Basic common metadata
            FillMetadata(matrix, dcm.Dataset);

            //RT Dose specific metadata including 3D dimensional specifications
            var zResolution = dcm.Dataset.GetValues<double>(DicomTag.GridFrameOffsetVector)[1] - dcm.Dataset.GetValues<double>(DicomTag.GridFrameOffsetVector)[0];
            matrix.CellSize = new Vector3f(matrix.CellSize.X, matrix.CellSize.Y, zResolution);

            matrix.DoseUnit = dcm.Dataset.GetValues<string>(DicomTag.Dose​Units)[0] == "GY" ? DoseUnit.ABSOLUTE :
                DoseUnit.RELATIVE;
            var sumType = dcm.Dataset.GetValues<string>(DicomTag.DoseSummationType)[0];
            matrix.SumType = sumType == "PLAN" ? DoseSumType.PLAN : DoseSumType.BEAM;
            matrix.PlanUID = dcm.Dataset.GetSequence(DicomTag.ReferencedRTPlanSequence)?.Items.FirstOrDefault()?
                .GetValues<string>(DicomTag.ReferencedSOPInstanceUID)[0];
            var dimZ = dcm.Dataset.GetValues<int>(DicomTag.NumberOfFrames)[0];
            matrix.Dimensions = new Vector3i(matrix.Dimensions.X, matrix.Dimensions.Y, dimZ);
            matrix.Resize();

            ////FILL VOXELS
            Func<BinaryReader, float> valueConverter = DoseToGrid.GetValueConverter(matrix);

            var m = dcm.Dataset.GetValues<double>(DicomTag.DoseGridScaling)[0];
            if (convertToRelative && matrix.DoseUnit != DoseUnit.RELATIVE && prescribedDoseGy.HasValue)
            {
                m = m / prescribedDoseGy.Value;
                matrix.DoseUnit = DoseUnit.RELATIVE;
            }

            var values = new List<float>();
            var pixelData = DicomPixelData.Create(dcm.Dataset);
            //Some stats we will gather here
            var maxDose = float.MinValue;
            var minDose = float.MaxValue;
            var maxDoseIndex = 0;
            var minDoseIndex = 0;
            var currentIndex = 0;
            for (int i = 0; i < pixelData.NumberOfFrames; i++)
            {
                var voxels = pixelData.GetFrame(i).Data;
                using (BinaryReader br = new BinaryReader(new MemoryStream(voxels)))
                {
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        var val = (float)(m * valueConverter(br));
                        if (val > maxDose)
                        {
                            maxDose = val;
                            maxDoseIndex = currentIndex;
                        }
                        if (val < minDose)
                        {
                            minDose = val;
                            minDoseIndex = currentIndex;
                        }
                        values.Add(val);
                        currentIndex++;
                    }
                }
            }

            matrix.MaxDose = maxDose;
            matrix.MinDose = minDose;
            matrix.MaxDosePoint = IndexUtil.ToGrid3Index(maxDoseIndex, matrix.Dimensions.X, matrix.Dimensions.Y);
            matrix.MinDosePoint = IndexUtil.ToGrid3Index(minDoseIndex, matrix.Dimensions.X, matrix.Dimensions.Y);
            Array.Copy(values.ToArray(), matrix.Buffer, matrix.Buffer.Length);
            return matrix;
        }
    }
}
