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
    public class EditCIViewModel : BindableBase
    {
        private ScoreMetricModel ScoreMetric;

        private string dose;

        public string Dose
        {
            get { return dose; }
            set 
            { 
                SetProperty( ref dose , value);
                ScoreMetric.InputValue = Dose;
            }
        }

        private string doseUnit;

        public string DoseUnit
        {
            get { return doseUnit; }
            set
            {
                SetProperty(ref doseUnit, value);
                ScoreMetric.InputUnit = DoseUnit;
            }
        }

        private ObservableCollection<string> doseUnits;

        public ObservableCollection<string> DoseUnits
        {
            get { return doseUnits; }
            set { SetProperty(ref doseUnits, value); }
        }

        private IEventAggregator EventAggregator;
        public EditCIViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            EventAggregator.GetEvent<ShowCIMetricEvent>().Subscribe(SetMetric);

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
            DoseUnit = scoreMetric.InputUnit;
            Dose = scoreMetric.InputValue;
        }
    }
}
