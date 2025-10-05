using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.Geometry
{
    public class OrientedByteGrid : OrientedGrid3f
    {
        public int BitsAllocated { get; set; }
        public int BytesAllocated => BitsAllocated / 8;
        public int BitsStored { get; set; }
    }
}
