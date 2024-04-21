using PlanScoreCard.Events.HelperWindows;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels.VMHelpers
{
    public class ConfigurationViewModel:BindableBase
    {
        private double _dvhResolution;
        private IEventAggregator _eventAggregator;

        public double DVHResolution
        {
            get { return _dvhResolution; }
            set { SetProperty(ref _dvhResolution,value); }
        }
        private bool _bStructureCreation;

        public bool bStructureCreation
        {
            get { return _bStructureCreation; }
            set { _bStructureCreation = value; }
        }
        private bool _bSaveStructures;

        public bool bSaveStructures
        {
            get { return _bSaveStructures; }
            set { _bSaveStructures = value; }
        }
        private bool _bNormCourse;

        public bool bNormCourse
        {
            get { return _bNormCourse; }
            set { SetProperty(ref _bNormCourse,value); }
        }
        private bool _bBatchNorm;

        public bool bBatchNorm
        {
            get { return _bBatchNorm; }
            set { _bBatchNorm = value; }
        }

        public bool bSave { get; set; }
        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public ConfigurationViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DVHResolution = Convert.ToDouble(ConfigurationManager.AppSettings["DVHResolution"]);
            bStructureCreation = ConfigurationManager.AppSettings["WriteEnabled"] == "true";
            bSaveStructures = ConfigurationManager.AppSettings["AddStructures"] == "true";
            bNormCourse = ConfigurationManager.AppSettings["NormCourse"] == "true";
            bBatchNorm = ConfigurationManager.AppSettings["BatchNorm"] == "true";

            SaveCommand = new DelegateCommand(OnSave);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        private void OnCancel()
        {
            _eventAggregator.GetEvent<ConfigurationCloseEvent>().Publish(this);
        }

        private void OnSave()
        {
            bSave = true;
            _eventAggregator.GetEvent<ConfigurationCloseEvent>().Publish(this);
        }
    }
}
