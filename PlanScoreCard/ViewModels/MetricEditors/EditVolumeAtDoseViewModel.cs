using PlanScoreCard.Events;
using PlanScoreCard.Models;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels.MetricEditors
{
    public class EditVolumeAtDoseViewModel : BindableBase, INotifyPropertyChanged
    {
        private ScoreMetricModel ScoreMetric;

        private string dose;

        public string Dose
        {
            get { return dose; }
            set
            {
                SetProperty(ref dose, value);
                ScoreMetric.InputValue = dose;
                EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
            }
        }

        private string doseUnit;

        public string DoseUnit
        {
            get { return doseUnit; }
            set
            {
                SetProperty(ref doseUnit, value);
                ScoreMetric.InputUnit = doseUnit;
                EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
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
                SetProperty(ref volumeUnit, value);
                ScoreMetric.OutputUnit = volumeUnit;
                EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
            }
        }

        private ObservableCollection<string> volumeUnits;

        public ObservableCollection<string> VolumeUnits
        {
            get { return volumeUnits; }
            set 
            { 
                SetProperty(ref volumeUnits, value);
                EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
            }

        }

        private IEventAggregator EventAggregator;

        public EditVolumeAtDoseViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            EventAggregator.GetEvent<ShowVolumeAtDoseMetricEvent>().Subscribe(SetMetric);

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

            Dose = scoreMetric.InputValue;
            VolumeUnit = scoreMetric.OutputUnit;
            DoseUnit = scoreMetric.InputUnit;
        }
    }
}
