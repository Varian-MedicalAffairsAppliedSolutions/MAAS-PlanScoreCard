﻿using PlanScoreCard.Events;
using PlanScoreCard.Models;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels.MetricEditors
{
    public class EditDoseAtVolumeViewModel : BindableBase
    {

        private ScoreMetricModel ScoreMetric;

        private string volume;

        public string Volume
        {
            get { return volume; }
            set 
            { 
                SetProperty(ref volume , value);
                ScoreMetric.InputValue = volume;
            }
        }

        private string doseUnit;

        public string DoseUnit
        {
            get { return doseUnit; }
            set 
            { 
                SetProperty(ref doseUnit, value);
                ScoreMetric.OutputUnit = doseUnit;
            }
        }

        private ObservableCollection<string> doseUnits;

        public ObservableCollection<string> DoseUnits
        {
            get { return doseUnits; }
            set { SetProperty(ref doseUnits, value); }
        }

        private string volumeUnit;

        public string VolumeUnit
        {
            get { return volumeUnit; }
            set 
            { 
                SetProperty(ref volumeUnit , value);
                ScoreMetric.InputUnit = volumeUnit;
            }
        }

        private ObservableCollection<string> volumeUnits;

        public ObservableCollection<string> VolumeUnits
        {
            get { return volumeUnits; }
            set { SetProperty(ref volumeUnits , value); }
        }

        private IEventAggregator EventAggregator;
        public EditDoseAtVolumeViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            EventAggregator.GetEvent<ShowDoseAtVolumeMetricEvent>().Subscribe(SetMetric);

            DoseUnits = new ObservableCollection<string>();
            VolumeUnits = new ObservableCollection<string>();

            var dUnits = ConfigurationManager.AppSettings["DoseUnits"].Split(';');
			var vUnits = ConfigurationManager.AppSettings["VolumeUnits"].Split(';');
			foreach (var du in dUnits)
			{
				DoseUnits.Add(du);
			}
			foreach (var vu in vUnits)
			{
				VolumeUnits.Add(vu);
			}
		}

        private void SetMetric(ScoreMetricModel scoreMetric)
        {
            ScoreMetric = scoreMetric;

            Volume = scoreMetric.InputValue;
            VolumeUnit = scoreMetric.InputUnit;
            DoseUnit = scoreMetric.OutputUnit;
        }
    }
}