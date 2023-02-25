using DVHViewer2.Views;
using PlanScoreCard.Models;
using PlanScoreCard.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VMS.TPS.Common.Model.API;

namespace DVHViewer2.ViewModels
{
    public class TabViewModel : BindableBase { 
    
        private ObservableCollection<TabItem> _Tabs;

        public DelegateCommand NewTabCmd { get; set; }

        private ProgressBar loadingBar;

        // Loading bar
        public ProgressBar LoadingBar
        {
            get { return loadingBar; }
            set { SetProperty(ref loadingBar, value); }
        }

        private int currentProgress;

        public int CurrentProgress
        {
            get { return currentProgress; }
            set { SetProperty(ref currentProgress, value); }
        }

        private bool showLoading;

        public bool ShowLoading
        {
            get { return showLoading; }
            set { SetProperty(ref showLoading, value); }
        }


        public DelegateCommand DeleteTabCmd { get; set; }

        private TabItem _CurrentTab;
        private ScoreCardModel _scoreCard;

        public TabItem CurrentTab
        {
            get { return _CurrentTab; }
            set { SetProperty(ref _CurrentTab, value); }
        }

        public ObservableCollection<TabItem> Tabs
        {
            get { return _Tabs; }
            set { SetProperty(ref _Tabs, value); }
        }

        public TabViewModel()
        {
            Tabs = new ObservableCollection<TabItem>();
            // Create button to launch new tab
            NewTabCmd = new DelegateCommand(OnNewTab);
            DeleteTabCmd = new DelegateCommand(OnDeleteTab, CanDeleteTab);

        }

        private bool CanDeleteTab()
        {
            if (CurrentTab == null)
            {
                return false;
            }
            return true;
        }

        private void OnDeleteTab()
        {
            Tabs.Remove(CurrentTab);
            
            if (Tabs.Count > 0)
            {
                CurrentTab = Tabs[Tabs.Count - 1];
            }
            else
            {
                CurrentTab = null;
            }
        }

        public void AddMultipleDVH(ScoreCardModel scoreCard, List<PlanningItem> planModels, StructureDictionaryService structureDictionaryService)
        {
            // Add multiple DVHs with progress bar
            // Show progress bar
            //LoadingBar = true;
            _scoreCard = scoreCard;
            //Dispatcher.BeginInvoke(new Action(delegate () { LoadingBar = true; }));
            //here progress bar is a UIElement
            // Add the dvh plots
            foreach (var plan in planModels)
            {
                AddDVH(plan, structureDictionaryService);
            }

        }

        // Method for adding a DVH tab, not from button click
        public void AddDVH(PlanningItem plan, StructureDictionaryService structureDictionaryService)
        {
            var t = new TabItem();
            t.Header = plan.Id;
            t.Content = new DVHView()
            {
                DataContext = new DVHViewModel((plan as ExternalPlanSetup), _scoreCard, structureDictionaryService)
            };
            Tabs.Add(t);
            CurrentTab= t;
        }

        private void OnNewTab()
        {
            var t = new TabItem();
            t.Header = "Hey there";
            CurrentTab = t;
            
            Tabs.Add(t);

        }
    }
}
