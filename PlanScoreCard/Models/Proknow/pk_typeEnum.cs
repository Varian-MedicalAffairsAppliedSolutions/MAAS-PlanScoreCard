using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models.Proknow
{
    public enum pk_typeEnum
    {
        DOSE_VOLUME_PERCENT_ROI,
        DOSE_VOLUME_CC_ROI,
        DOSE_VOLUME_MINUS_CC_ROI,
        VOLUME_CC_DOSE_ROI,
        VOLUME_PERCENT_DOSE_ROI,
        VOLUME_PERCENT_DOSE_RANGE_ROI,
        VOLUME_CC_DOSE_RANGE_ROI,
        MIN_DOSE_ROI,
        MAX_DOSE_ROI,
        MEAN_DOSE_ROI,
        INTEGRAL_DOSE_ROI,
        MAX_DOSE,
        VOLUME_OF_REGRET,
        IRRADIATED_VOLUME,
        CONFORMATION_NUMBER,
        CONFORMALITY_INDEX,
        HOMOGENEITY_INDEX,
        INHOMOGENEITY_INDEX,
        CUMULATIVE_METERSET,
        VOLUME

    }
}
