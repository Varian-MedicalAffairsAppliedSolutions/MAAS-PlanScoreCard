using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models
{
    public enum MetricTypeEnum
    {
        MinDose,
        MaxDose,
        MeanDose,
        DoseAtVolume,
        VolumeAtDose,
        Volume,
        VolumeOfRegret,
        ConformationNumber,
        HomogeneityIndex,
        ConformityIndex,
        Undefined,
    }
}
