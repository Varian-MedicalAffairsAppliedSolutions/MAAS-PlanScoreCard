using PlanScoreCard.Models.Internals;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DVHViewer2.Models
{
    public class MetricPlotItem : BindableBase
    {
        public ScoreTemplateModel stm { get; set; }
        public string id { get; set; }
        private DVHPlotModel plotmodel { get; set; }

        private ScoreBarModel sb;
        private StructurePlotItem parent;
        public DelegateCommand CheckedCommand { get; set; }
        
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set { SetProperty(ref isChecked, value); }
        }
        public MetricPlotItem(ScoreTemplateModel stm, DVHPlotModel plotmodel, Structure structure, double rx_dose, StructurePlotItem parent, String planDoseUnit)
        {
            this.stm = stm;
            this.id = $"[{stm.OutputUnit}]{stm.MetricType}({stm.InputValue}{stm.InputUnit})";
            this.plotmodel = plotmodel;
            this.sb = new ScoreBarModel(stm, rx_dose, structure, planDoseUnit);
            this.parent = parent;

            CheckedCommand = new DelegateCommand(OnChecked);
        }

        private void OnChecked()
        {
            plotmodel.PlotScoreBarForMetric(sb, IsChecked);

        }
    }
}
