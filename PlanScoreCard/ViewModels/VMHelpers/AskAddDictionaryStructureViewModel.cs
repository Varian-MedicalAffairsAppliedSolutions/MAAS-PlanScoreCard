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
    public class AskAddDictionaryStructureViewModel:BindableBase
    {
        private IEventAggregator _eventAggregator;
        private string _newStructure;

        public DelegateCommand YesCommand { get; set; }
        public DelegateCommand NoCommand { get; set; }
        public DelegateCommand ExcludeCommand { get; set; }
        public AskAddDictionaryStructureViewModel(IEventAggregator eventAggregator,String newStructure)
        {
            _eventAggregator = eventAggregator;
            _newStructure = newStructure;

            YesCommand = new DelegateCommand(OnYes);
            NoCommand = new DelegateCommand(OnNo);
            ExcludeCommand = new DelegateCommand(OnExclude);
        }

        private void OnExclude()
        {
            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["DictionaryExclusions"]))
            {
                ConfigurationManager.AppSettings["DictionaryExclusions"] = _newStructure;
            }
            else
            {
                ConfigurationManager.AppSettings["DictionaryExclusions"] += $";{_newStructure}";
            }
            OnNo();
        }

        private void OnNo()
        {
            _eventAggregator.GetEvent<NoEvent>().Publish();
        }

        private void OnYes()
        {
            _eventAggregator.GetEvent<YesEvent>().Publish();
        }
    }
}
