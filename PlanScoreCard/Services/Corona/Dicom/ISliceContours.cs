using CoronaDVH.GMath;
using System.Collections.Generic;

namespace CoronaDVH.Dicom
{
    public interface ISliceContours
    {
        Dictionary<double, List<PolyLine2d>> Contours { get; set; }
    }
}
