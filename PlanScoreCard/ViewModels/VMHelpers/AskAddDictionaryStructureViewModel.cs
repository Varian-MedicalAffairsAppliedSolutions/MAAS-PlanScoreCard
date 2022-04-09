using PlanScoreCard.Events.HelperWindows;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels.VMHelpers
{
    public class AskAddDictionaryStructureViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator;
        private string _newStructure;

        public DelegateCommand YesCommand { get; set; }
        public DelegateCommand NoCommand { get; set; }
        public DelegateCommand ExcludeCommand { get; set; }
        public AskAddDictionaryStructureViewModel(IEventAggregator eventAggregator, String newStructure)
        {
            _eventAggregator = eventAggregator;
            _newStructure = newStructure;

            YesCommand = new DelegateCommand(OnYes);
            NoCommand = new DelegateCommand(OnNo);
            ExcludeCommand = new DelegateCommand(OnExclude);
        }

        private void OnExclude()
        {
            //check if config can be modified
            var configPath = Assembly.GetExecutingAssembly().Location;
            using (var fileStream = new FileStream(configPath, FileMode.Open))
            {
                if (!fileStream.CanWrite)
                {
                    System.Windows.MessageBox.Show($"Cannot update config file. \nUser does not have rights to {configPath}");
                    return;
                }
            }
            //this needs to be the path running the application
            var configFile = ConfigurationManager.OpenExeConfiguration(configPath);
           
            var value = ConfigurationManager.AppSettings["DictionaryExclusions"];
            var splitValues = value.Split(';');
            if (!splitValues.Any(x => x.Equals(_newStructure, StringComparison.OrdinalIgnoreCase)))
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
            ConfigurationManager.RefreshSection("appSettings");
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
