using CoronaDVH.Geometry;
using CoronaDVH.GMath;
using CoronaDVH.Helpers;
using System;
using System.Linq;
using System.Runtime.ConstrainedExecution;

namespace CoronaDVH.Dicom
{
    public static class StructureMask
    {
        // Create a fractional mask by oversampling the dose grid.
        public static OrientedGrid3f FromContours(RTStructure str, OrientedGrid3f dose, int oversampleFactor = 4)
        {
            // Determine grid geometry from the dose volume
            var baseSpacing = dose.CellSize;
            var highResSpacing = baseSpacing / oversampleFactor;
            highResSpacing.Z = baseSpacing.Z; // Keep Z spacing unchanged

            var origin = dose.Orientation.Origin;
            // Oversample only in X and Y:
            int nx = dose.Dimensions[0] * oversampleFactor;
            int ny = dose.Dimensions[1] * oversampleFactor;
            // Keep the Z resolution the same:
            int nz = dose.Dimensions[2];

            var highResMask = new OrientedGrid3f(nx, ny, nz, 0);
            highResMask.CellSize = highResSpacing;
            highResMask.Orientation = dose.Orientation;

            // Use half the base slice thickness as the tolerance for matching contours
            float sliceTolerance = baseSpacing.Z / 2f;

            // Iterate over each Z slice of the high-res grid (Z is not oversampled)
            for (int iz = 0; iz < nz; ++iz)
            {
                // Compute world Z for the center of the current slice
                var sliceZCenter = highResMask.GridToWorld(new Vector3i(0, 0, iz)).Z;

                // Get contours whose Z coordinate is within the tolerance of this slice:
                var sliceContours = str.Contours
                    .Where(k => Math.Abs(k.Key - sliceZCenter) <= sliceTolerance);
                if (!sliceContours.Any())
                {
                    continue; // Skip if no contour is available for this slice
                }

                // Choose the contour set that is closest to the slice center:
                var closest = sliceContours.OrderBy(k => Math.Abs(k.Key - sliceZCenter)).FirstOrDefault();

                foreach (var contour in closest.Value)
                {
                    // Transform contour points from world coordinates to high-res grid coordinates
                    var contourPts = contour
                        .Select(pt => highResMask.WorldToGrid(new Vector3f((float)pt.X, (float)pt.Y, sliceZCenter)))
                        .Select(pt => new Vector2d(pt.X, pt.Y))
                        .ToList();
                    PolyFill.FillSlice(highResMask, new PolyLine2d(contourPts), iz, 1f);
                }
            }

            // Downsample the high-res mask back to the original dose grid resolution.
            // Since we've oversampled only in X and Y, average over oversampleFactor*oversampleFactor samples.
            var fracMask = dose.EmptyClone();
            for (int i = 0; i < dose.Dimensions[0]; ++i)
            {
                for (int j = 0; j < dose.Dimensions[1]; ++j)
                {
                    for (int k = 0; k < dose.Dimensions[2]; ++k)
                    {
                        int startX = i * oversampleFactor;
                        int startY = j * oversampleFactor;
                        int startZ = k; // Z is not oversampled
                        int countInside = 0;
                        int total = oversampleFactor * oversampleFactor; // Only oversampling in XY

                        for (int ii = 0; ii < oversampleFactor; ++ii)
                        {
                            for (int jj = 0; jj < oversampleFactor; ++jj)
                            {
                                if (highResMask[startX + ii, startY + jj, startZ] > 0.5f)
                                {
                                    countInside++;
                                }
                            }
                        }
                        fracMask[i, j, k] = (float)countInside / total;
                    }
                }
            }
            return fracMask;
        }

    }

}

