using System.Collections.Generic;
using System.Linq;

namespace CoronaDVH.Models
{
    public class DvhData
    {
        /// <summary>
        /// Discrete DVH data points (not cumulative)
        /// </summary>
        public List<DvhPoint> Points { get; set; } = new List<DvhPoint>();
        public float MaxDose { get; set; }
        public float MinDose { get; set; }
        public float MeanDose { get; set; }
        public float Volume { get; internal set; }

        /// <summary>
        /// Samples the dose at a given volume for a cumulative DVH.
        /// </summary>
        /// <param name="sampleVolume">volume in the same units as the points</param>
        /// <returns>dose in the same units as the points</returns>
        public float GetDoseAtVolume(float sampleVolume)
        {
            // Safety check: if there are no points, return 0 (or throw an exception).
            if (!Points.Any())
                return 0f;

            // Sort points descending by dose.
            var sortedPoints = Points.OrderByDescending(p => p.Dose).ToList();

            // Compute cumulative volumes for the sorted list.
            // cumVolumes[i] is the cumulative volume from index 0 up to i.
            float cumulativeVolume = 0f;
            var cumList = new List<(float Dose, float CumVolume)>(sortedPoints.Count);
            foreach (var pt in sortedPoints)
            {
                cumulativeVolume += pt.Volume;
                cumList.Add((pt.Dose, cumulativeVolume));
            }

            // If the requested sample volume is <= 0, return the highest dose.
            if (sampleVolume <= 0)
                return sortedPoints.First().Dose;

            // If sampleVolume exceeds the total volume, return the lowest dose.
            if (sampleVolume >= cumulativeVolume)
                return sortedPoints.Last().Dose;

            // Find the first index where the cumulative volume equals or exceeds the sample volume.
            for (int i = 0; i < cumList.Count; i++)
            {
                if (cumList[i].CumVolume >= sampleVolume)
                {
                    // If it is the first point, just return its dose.
                    if (i == 0)
                        return cumList[i].Dose;

                    // Otherwise, interpolate linearly between the previous and current point.
                    var (doseHigh, volHigh) = cumList[i - 1];
                    var (doseLow, volLow) = cumList[i];

                    // Calculate the fraction between volHigh and volLow that gives us sampleVolume.
                    float fraction = (sampleVolume - volHigh) / (volLow - volHigh);

                    // Since dose decreases as cumulative volume increases, we linearly interpolate.
                    // dose = doseHigh + fraction*(doseLow - doseHigh)
                    return doseHigh + fraction * (doseLow - doseHigh);
                }
            }

            // Fallback (should not be reached because of earlier checks).
            return sortedPoints.Last().Dose;
        }

        /// <summary>
        /// Samples the volume at a given dose for a cumulative DVH.
        /// </summary>
        /// <param name="dose">dose in the same units as the points</param>
        /// <returns>volume in the same units as the points</returns>
        public float GetVolumeAtDose(float dose)
        {
            // Sum all volumes for which the dose is at least the specified threshold.
            return Points.Where(p => p.Dose >= dose).Sum(p => p.Volume);
        }

    }
}
