using PlanScoreCard.Events.HelperWindows;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
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
            //this needs to be the path running the application
            var configFile = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            var value = ConfigurationManager.AppSettings["DictionaryExclusions"];
            var splitValues = value.Split(';');
            if (!splitValues.Any(x=>x.Equals(_newStructure, StringComparison.OrdinalIgnoreCase)))
            {
                if (String.IsNullOrEmpty(value))
                {
                    //ConfigurationManager.AppSettings["DictionaryExclusions"] = _newStructure;
                    configFile.AppSettings.Settings.Remove("DictionaryExclusions");
                    configFile.AppSettings.Settings.Add("DictionaryExclusions", _newStructure);
                }
                else
                {

                    configFile.AppSettings.Settings.Remove("DictionaryExclusions");
                    configFile.AppSettings.Settings.Add("DictionaryExclusions", $"{value};{_newStructure}");
                }
            }
            configFile.Save(ConfigurationSaveMode.Modified);
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
