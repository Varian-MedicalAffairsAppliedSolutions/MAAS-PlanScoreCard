using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PlanScoreCard.Events;
using PlanScoreCard.Events.Plugin;
using PlanScoreCard.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PlanScoreCard.ViewModels
{
    public class PluginViewModel : BindableBase
    {
        private string _consoleOutput;
        private IEventAggregator _eventAggregator;
        private ViewLauncherService ViewLauncherService;

        public string ConsoleOutput
        {
            get { return _consoleOutput; }
            set { SetProperty(ref _consoleOutput, value); }
        }
        private bool _breloadAvailable;

        public bool bReloadAvailable
        {
            get { return _breloadAvailable; }
            set { SetProperty(ref _breloadAvailable, value); }
        }
        private bool _bOptimizeAvailable;

        public bool bOptimizeAvailable
        {
            get { return _bOptimizeAvailable; }
            set { SetProperty(ref _bOptimizeAvailable, value); }
        }
        private bool _bFinalizeAvailable;

        public bool bFinalizeAvailable
        {
            get { return _bFinalizeAvailable; }
            set { SetProperty(ref _bFinalizeAvailable, value); }
        }

        private UserControl pluginConsoleUserControl;

        public UserControl PluginConsoleUserControl
        {
            get { return pluginConsoleUserControl; }
            set { SetProperty<UserControl>(ref pluginConsoleUserControl, value); }
        }

        private UserControl pluginPlotUserControl;

        public UserControl PluginPlotUserControl
        {
            get { return pluginPlotUserControl; }
            set { SetProperty<UserControl>(ref pluginPlotUserControl, value); }
        }


        public DelegateCommand ReloadAppCommand { get; private set; }
        public DelegateCommand OptimizeCommand { get; private set; }
        public DelegateCommand FinalizeCommand { get; private set; }
        public PlotModel PlotData { get; set; }
        public PluginViewModel(IEventAggregator eventAggregator, ViewLauncherService viewLauncherService)
        {
            _eventAggregator = eventAggregator;
            ViewLauncherService = viewLauncherService;
            //Feedbacks = new List<string>();
            PlotData = new PlotModel() { LegendPosition = LegendPosition.RightTop };
            ReloadAppCommand = new DelegateCommand(OnReloadApp);
            OptimizeCommand = new DelegateCommand(OnOptimize);
            FinalizeCommand = new DelegateCommand(OnFinalize);

        }

        private void OnFinalize()
        {
            //_eventAggregator.GetEvent<FinalizePluginEvent>().Publish(null);
        }

        private void OnOptimize()
        {
            //_eventAggregator.GetEvent<OptimizePluginEvent>().Publish(null);
        }

        private void OnPluginRun(bool obj)
        {
            ConsoleOutput = String.Empty;
            bReloadAvailable = false;
        }

        private void OnReloadApp()
        {
            //_eventAggregator.GetEvent<ResetApplicationEvent>().Publish(Feedbacks);
            //Feedbacks.Clear();
        }
        public string Feedbacks { get; private set; }

        private void OnConsoleUpdate(string obj)
        {
            if (obj.Contains("Activate"))
            {
                Feedbacks = obj;
            }
            //ReloadAppCommand.RaiseCanExecuteChanged();
            //if (obj.Contains("Finished Initialize")) 
            //{ 
            //    bOptimizeAvailable = true; 
            //    if(ConfigurationManager.AppSettings["OptMode"] == "Auto")
            //    {
            //        OnOptimize();
            //    }
            //}
            //if(obj.Contains("Finished Optimize")) 
            //{ 
            //    bFinalizeAvailable = true; 
            //    if(ConfigurationManager.AppSettings["OptMode"] == "Auto")
            //    {
            //        OnFinalize();
            //    }
            //}
            //if (obj.Contains("Activate")||obj.Contains("Finished Finalize")) 
            //{
            //    Thread.Sleep(8000);
            //    bReloadAvailable = true; 
            //}
            ConsoleOutput += System.Environment.NewLine + obj;
        }
    }
}
