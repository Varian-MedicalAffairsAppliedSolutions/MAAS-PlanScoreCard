﻿using Prism.Mvvm;

namespace PlanScoreCard.Models
{
    public class ScoreValueModel:BindableBase
    {
        public int MetricId { get; set; }
        public string PlanId { get; set; }
        public double Value { get; set; }
        public double Score { get; set; }
        public string CourseId { get; set; }
        public string OutputUnit { get; set; }
        public string PatientId { get; set; }
        public string StructureId { get; set; }
        public int TemplateNumber { get; set; }
        public bool SharedStructureId { get; set; }
    }
}
