using PlanScoreCard.Events;
using PlanScoreCard.Models.Internals;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private bool _bPluginVisibility;

        public bool bPluginVisibility
        {
            get { return _bPluginVisibility; }
            set { SetProperty(ref _bPluginVisibility, value); }
        }
        private int _pluginWidth;

        public int PluginWidth
        {
            get { return _pluginWidth; }
            set { SetProperty(ref _pluginWidth, value); }
        }


        public MainViewModel(NavigationViewModel navigationViewModel,
            PlanScoreViewModel planScoreViewModel,
            Patient patient,
            Course course,
            PlanSetup plan,
            Application app, 
            IEventAggregator eventAggregator)
        {
            _patientId = patient.Id;
            _courseId = course.Id;
            _planId = plan.Id;
            _app = app;
            NavigationViewModel = navigationViewModel;
            PlanScoreViewModel = planScoreViewModel;
            bPluginVisibility = false;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ScorePlanEvent>().Subscribe(OnScorePlan);
        }

        private void OnScorePlan(List<ScoreTemplateModel> templates)
        {
            if (NavigationViewModel.GenScoreCardView != null)
            {
                NavigationViewModel.UpdateScoreTemplates(templates);
                NavigationViewModel.GenScoreCardView.Close();
            }
            PlanScoreViewModel.ScorePlan(templates);
        }

        private string _patientId;
        private string _courseId;
        private string _planId;
        private Application _app;

        public NavigationViewModel NavigationViewModel { get; }
        public PlanScoreViewModel PlanScoreViewModel { get; }

        private IEventAggregator _eventAggregator;
    }
}
