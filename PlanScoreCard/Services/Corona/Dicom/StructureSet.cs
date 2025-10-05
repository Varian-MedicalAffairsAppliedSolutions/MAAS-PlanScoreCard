using CoronaDVH.GMath;
using CoronaDVH.Helpers;
using Dicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CoronaDVH.Dicom
{
    public class StructureSet
    {
        public static List<RTStructure> FromFile(string structureFile)
        {
            //var logger = ServiceLocator.ServiceProvider.GetRequiredService<ILogger<StructureSet>>();
            var dcm = DicomFile.Open(structureFile);
            var metas = dcm.Dataset.GetSequence(DicomTag.StructureSetROISequence).Items.Select(i =>
            {
                var meta = new RTStructure();
                meta.StructureId = i.GetString(DicomTag.ROIName);
                meta.ROINumber = i.GetValues<int>(DicomTag.ROINumber)[0];
                return meta;
            }).ToList();

            for (int k = 0; k < metas.Count; k++)
            {
                var meta = metas[k];
                try
                {
                    var comatch = dcm.Dataset.GetSequence(DicomTag.ROIContourSequence)
                        .Items.FirstOrDefault(i => i.GetValues<int>(DicomTag.ReferencedROINumber)[0] == meta.ROINumber);
                    var romatch = dcm.Dataset.GetSequence(DicomTag.RTROIObservationsSequence)
                        .Items.FirstOrDefault(i => i.GetValues<int>(DicomTag.ReferencedROINumber)[0] == meta.ROINumber);

                    var colorValues = comatch.GetValues<int>(DicomTag.ROIDisplayColor);
                    meta.Color = new RgbaColor((byte)colorValues[0], (byte)colorValues[1], (byte)colorValues[2]);
                    meta.DicomType = romatch.GetString(DicomTag.RTROIInterpretedType);
                    meta.Name = romatch.GetString(DicomTag.ROIObservationLabel);
                    //meta.Name = romatch.GetString(DicomTag.ROIObservationLabelRETIRED);

                    var hasContours = comatch.GetSequence(DicomTag.ContourSequence) != null;
                    if (!hasContours) { continue; }

                    Dictionary<double, List<PolyLine2d>> allSliceContours = new Dictionary<double, List<PolyLine2d>>();
                    //HAS CONTOURS - SET COLOR BYTES IN MATRIX
                    foreach (var slice in comatch.GetSequence(DicomTag.ContourSequence).Items)
                    {
                        var contours = slice.GetValues<float>(DicomTag.ContourData);
                        if (contours.Length < 2 || contours.Length % 3 != 0)
                        {
                            //logger.LogError($"Slice for structure {meta.StructureId} has {contours.Length} contour points. Not divisible by 3! Can't process."); continue;
                        }
                        try
                        {
                            List<PolyLine2d> sliceContours = null;
                            if (allSliceContours.ContainsKey(contours[2]))
                            {
                                sliceContours = allSliceContours[contours[2]];
                            }
                            else
                            {
                                sliceContours = new List<PolyLine2d>();
                                allSliceContours.Add(contours[2], sliceContours);
                            }

                            var closedContour = new List<Vector2d>();
                            for (int i = 0; i < contours.Length; i += 3)
                            {
                                var contourPt = new Vector3f(contours[i + 0], contours[i + 1], contours[i + 2]);
                                closedContour.Add(new Vector2d(contourPt.X, contourPt.Y));
                            }
                            sliceContours.Add(new PolyLine2d(closedContour));
                            allSliceContours[contours[2]] = sliceContours;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                            //logger.LogError(e.ToString());
                        }
                    }
                    meta.Contours = allSliceContours;
                    metas[k] = meta;
                }
                catch (Exception e)
                {
                    //logger.LogError(e, $"Could not add structure {meta.StructureId}");
                    MessageBox.Show($"Could not add structure {meta.StructureId}");
                }
            }
            return metas;
        }
    }
}
