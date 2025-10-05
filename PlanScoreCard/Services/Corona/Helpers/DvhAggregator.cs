using CoronaDVH.Geometry;
using CoronaDVH.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaDVH.Helpers
{
    public class DvhAggregator
    {
        public static DvhData Aggregate(OrientedGrid3f dose, OrientedGrid3f mask)
        {
            var dvh = new DvhData();
            // Initialize extremes so that the reduction can work correctly
            dvh.MinDose = float.MaxValue;
            dvh.MaxDose = float.MinValue;

            // Compute the full voxel volume from the mask's cell size
            float fullVoxelVol = mask.CellSize.X * mask.CellSize.Y * mask.CellSize.Z;

            // Total number of voxels in the mask
            int totalVoxels = mask.Dimensions.X * mask.Dimensions.Y * mask.Dimensions.Z;

            // Thread-safe container for DVH points
            var dvhPoints = new ConcurrentBag<DvhPoint>();

            // Global accumulators (to be combined from thread-local values)
            double globalVolume = 0.0;
            double globalWeightedDose = 0.0;
            float globalMinDose = float.MaxValue;
            float globalMaxDose = float.MinValue;

            // We'll use a lock when updating the global accumulators.
            object syncObj = new object();

            // Use Parallel.For with thread-local state
            Parallel.For(0, totalVoxels,
                // Initialize thread-local state
                () => new {
                    volume = 0.0,
                    weightedDose = 0.0,
                    minDose = float.MaxValue,
                    maxDose = float.MinValue
                },
                // Loop body: process each voxel by its linear index
                (i, loopState, local) =>
                {
                    // Convert linear index to voxel coordinate
                    var xyz = mask.ToIndex(i);
                    float fraction = mask[xyz]; // fractional occupancy
                    float voxelVol = fullVoxelVol * fraction;
                    if (voxelVol > 0)
                    {
                        // Read the corresponding dose value
                        float doseVal = dose[xyz];
                        dvhPoints.Add(new DvhPoint(doseVal, voxelVol));

                        // Update thread-local accumulators
                        local = new
                        {
                            volume = local.volume + voxelVol,
                            weightedDose = local.weightedDose + doseVal * voxelVol,
                            minDose = Math.Min(local.minDose, doseVal),
                            maxDose = Math.Max(local.maxDose, doseVal)
                        };
                    }
                    return local;
                },
                // Final action: combine each thread's local state into global accumulators
                local =>
                {
                    lock (syncObj)
                    {
                        globalVolume += local.volume;
                        globalWeightedDose += local.weightedDose;
                        globalMinDose = Math.Min(globalMinDose, local.minDose);
                        globalMaxDose = Math.Max(globalMaxDose, local.maxDose);
                    }
                }
            );

            // Save the computed DVH points and accumulate totals into the result.
            dvh.Points = dvhPoints.ToList();
            dvh.Volume = (float)globalVolume;
            dvh.MeanDose = globalVolume > 0 ? (float)(globalWeightedDose / globalVolume) : 0.0f;
            dvh.MinDose = globalMinDose;
            dvh.MaxDose = globalMaxDose;

            return dvh;
        }

    }
}
