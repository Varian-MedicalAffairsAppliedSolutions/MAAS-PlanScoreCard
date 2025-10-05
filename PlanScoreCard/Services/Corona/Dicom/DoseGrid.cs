using CoronaDVH.Geometry;
using CoronaDVH.GMath;

namespace CoronaDVH.Dicom
{
    public enum DoseSumType
    {
        PLAN,
        BEAM
    }
    public enum DoseUnit
    {
        ABSOLUTE,
        RELATIVE
    }

    public class DoseGrid : OrientedByteGrid
    {
        public double? PrescriptionDoseGy { get; set; }
        public DoseUnit DoseUnit { get; set; }
        public DoseSumType SumType { get; set; }
        public string PlanUID { get; set; }
        public float MaxDose { get; set; }
        public Vector3i MaxDosePoint { get; set; }
        public float MinDose { get; set; }
        public Vector3i MinDosePoint { get; set; }
    }
}
