using System.Linq;
using Prism.Mvvm;
using VMS.TPS.Common.Model.API;
using Prism.Commands;
using PlanScoreCard.Models.Internals;
using Newtonsoft.Json;
using System.IO;
using DVHViewer2.Models;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using PlanScoreCard.Models;
using PlanScoreCard.Services;

// TODO 2.21
// 6. Mahapps?
// 5. Maybe add polyline tooltip on scorebar?




namespace DVHViewer2.ViewModels
{
    public class DVHViewModel : BindableBase
    {
        // This contains the plot items to display in list view
        public ObservableCollection<StructurePlotItem> StructurePlotItems { get; set; }

        private ExternalPlanSetup _Plan;

        public ExternalPlanSetup Plan
        {
            get { return _Plan; }
            set { SetProperty(ref _Plan, value); }
        }


        private DVHPlotModel _Plot;
        public DVHPlotModel Plot                                                                    
        {
            get { return _Plot; }
            set { SetProperty(ref _Plot, value); }
        }

        public DelegateCommand ImportScorecard { get; private set; }


        // Override of ctor for creating a DVH w scorecard provided right away
        public DVHViewModel(ExternalPlanSetup Plan, ScoreCardModel scoreCard)//, StructureDictionaryService structureDictionaryService)
        {
            _Plan = Plan;
            //InternalTemplateModel template = JsonConvert.DeserializeObject<InternalTemplateModel>(File.ReadAllText(FileName));
            _Plot = new DVHPlotModel(Plan, scoreCard, Plan.StructureSet.Structures);//, structureDictionaryService);


            StructurePlotItems = _Plot.GetPlotItems(_Plot); // Get this once cuz it wont change

            // Check the first item
            StructurePlotItems.First().IsChecked = true;
            StructurePlotItems.First().CheckedCommand.Execute();

            // TODO build plot items here

            // Plot DVH curves
            //_Plot.PlotDVHCurves(_Plan);
            // Plot Score Bars
            //var Rx = _Plan.RTPrescription.Targets.First().DosePerFraction.Dose * _Plan.RTPrescription.Targets.First().NumberOfFractions;
            //_Plot.PlotScoreBars(template, 34, new List<Structure>(_Plan.StructureSet.Structures));

        }


        //private bool CanImportScorecard()
        //{
        //    return true;
        //}

        //private void OnImportScorecard()
        //{
        //    OpenFileDialog ofd = new OpenFileDialog();
        //    ofd.Filter = "JSON Template (*.json)|*.json|ePeer Review(*.csv)|*.csv";
        //    ofd.Title = "Open PlanScore Template";
        //    if (ofd.ShowDialog() != true || ofd.FileName == null)
        //    {
        //        return;
        //    }

        //    InternalTemplateModel template = JsonConvert.DeserializeObject<InternalTemplateModel>(File.ReadAllText(ofd.FileName));

        //    // Plot DVH curves
        //    //_Plot.PlotDVHCurves(_Plan);
        //    // Plot Score Bars
        //    //var Rx = _Plan.RTPrescription.Targets.First().DosePerFraction.Dose * _Plan.RTPrescription.Targets.First().NumberOfFractions;
        //    //_Plot.PlotScoreBars(template, 34, new List<Structure>(_Plan.StructureSet.Structures));
            
        //} 
    }
}
