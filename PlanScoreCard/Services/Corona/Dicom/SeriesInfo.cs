using CoronaDVH.GMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.Dicom
{
    public class SeriesInfo
    {
        public string SeriesUID { get; set; }
        public Dictionary<double, string> SliceMap { get; set; } = new Dictionary<double, string>();
        public double SliceThickness { get; set; }
        public int XSize { get; set; }
        public int YSize { get; set; }
        public int ZSize { get; set; }
        public double Z0 { get; set; }
        public Vector3f ZDir { get; set; }
    }
}
