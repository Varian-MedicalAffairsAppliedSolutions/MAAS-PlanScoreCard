using CoronaDVH.Geometry;
using CoronaDVH.GMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.Dicom
{
    public class RTStructure
    {
        public Dictionary<double, List<PolyLine2d>> Contours { get; set; } = new Dictionary<double, List<PolyLine2d>>();
        public string StructureId { get; set; }
        public RgbaColor Color { get; set; }
        public int ROINumber { get; set; }
        public string DicomType { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return StructureId;
        }
    }
}
