using PlanScoreCard.Events;
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
    public class EditDoseValueViewModel : BindableBase
    {
        private ScoreMetricModel ScoreMetric;

        private string doseUnit;

        public string DoseUnit
        {
            get { return doseUnit; }
            set
            {
                SetProperty(ref doseUnit, value);
                ScoreMetric.OutputUnit = DoseUnit;
                EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
            }
        }

        private ObservableCollection<string> doseUnits;

        public ObservableCollection<string> DoseUnits
        {
            get { return doseUnits; }
            set 
            { 
                SetProperty(ref doseUnits, value);
                EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
            }
        }

        private IEventAggregator EventAggregator;
        public EditDoseValueViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            EventAggregator.GetEvent<ShowDoseValueMetricEvent>().Subscribe(SetMetric);

            DoseUnits = new ObservableCollection<string>();

            var dUnits = ConfigurationManager.AppSettings["DoseUnits"].Split(';');
            var vUnits = ConfigurationManager.AppSettings["VolumeUnits"].Split(';');
            foreach (var du in dUnits)
            {
                DoseUnits.Add(du);
            }
        }

        private void SetMetric(ScoreMetricModel scoreMetric)
        {
            ScoreMetric = scoreMetric;
            DoseUnit = scoreMetric.OutputUnit;
        }
    }
}
