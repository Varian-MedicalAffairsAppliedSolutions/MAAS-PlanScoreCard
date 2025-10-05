using CoronaDVH.Geometry;
using CoronaDVH.GMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoronaDVH.Helpers
{
    public class PolyFill
    {
        /// <summary>
        /// Fills a single slice (2D grid) of an OrientedGrid3f using a polygon defined by poly.
        /// The polygon is assumed to be in grid coordinates.
        /// </summary>
        public static void FillSlice(OrientedGrid3f grid, PolyLine2d poly, int sliceZ, float value)
        {
            // Get all segments that are not horizontal (avoid division by zero)
            var segments = poly.GetSegments()
                               .Where(s => Math.Abs(s.Direction.Y) > 1e-8)
                               .ToList();

            // Determine vertical bounds (in grid coordinates) from the polygon points
            double minY = poly.Min(pt => pt.Y);
            double maxY = poly.Max(pt => pt.Y);

            // Iterate over each row (y) of the grid covered by the polygon.
            // Using y + 0.5 gives the center of the pixel row.
            for (int y = (int)Math.Floor(minY); y < (int)Math.Ceiling(maxY); ++y)
            {
                double scanY = y + 0.5;
                List<double> xIntersections = new List<double>();

                // Process each segment
                foreach (var seg in segments)
                {
                    // Order the segment endpoints so that p0.Y <= p1.Y.
                    Vector2d p0 = seg.P0, p1 = seg.P1;
                    if (p0.Y > p1.Y)
                    {
                        p0 = seg.P1;
                        p1 = seg.P0;
                    }

                    // Use a top-inclusive, bottom-exclusive test to avoid double counting:
                    // include the intersection if scanY is >= p0.Y and < p1.Y.
                    if (scanY >= p0.Y && scanY < p1.Y)
                    {
                        // Compute the intersection's x coordinate using linear interpolation.
                        double t = (scanY - p0.Y) / (p1.Y - p0.Y);
                        double intersectX = p0.X + t * (p1.X - p0.X);
                        xIntersections.Add(intersectX);
                    }
                }

                // Sort the intersections by x coordinate.
                xIntersections.Sort();

                // There should be an even number of intersections for a closed polygon.
                if (xIntersections.Count % 2 != 0)
                {
                    // Optionally log or handle the anomaly. Here we skip this row.
                    continue;
                }

                // Fill between pairs of intersections.
                for (int i = 0; i < xIntersections.Count; i += 2)
                {
                    double startX = xIntersections[i];
                    double endX = xIntersections[i + 1];

                    // Determine pixel indices to fill.
                    // Using ceiling for start and floor for end ensures we fill only fully covered pixels.
                    int ixStart = (int)Math.Ceiling(startX);
                    int ixEnd = (int)Math.Floor(endX);

                    // If the intersection falls within a pixel, you may choose to fill that pixel.
                    // Here, we fill all pixels between the computed indices.
                    for (int x = ixStart; x <= ixEnd; x++)
                    {
                        // Ensure the index is within grid bounds.
                        if (x >= 0 && x < grid.Dimensions.X && y >= 0 && y < grid.Dimensions.Y)
                        {
                            grid[x, y, sliceZ] = value;
                        }
                    }
                }
            }
        }
    }
}
