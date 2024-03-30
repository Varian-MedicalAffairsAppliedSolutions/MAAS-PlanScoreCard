using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Models.ModelHelpers;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels.VMHelpers
{
    public class AsymmetricMarginViewModel : BindableBase
    {
        private int _leftMargin;

        public int LeftMargin
        {
            get { return _leftMargin; }
            set { SetProperty(ref _leftMargin, value); }
        }
        private int _rightMargin;

        public int RightMargin
        {
            get { return _rightMargin; }
            set { SetProperty(ref _rightMargin, value); }
        }
        private int _supMargin;

        public int SupMargin
        {
            get { return _supMargin; }
            set { SetProperty(ref _supMargin, value); }
        }
        private int _infMargin;

        public int InfMargin
        {
            get { return _infMargin; }
            set { SetProperty(ref _infMargin, value); }
        }
        private int _postMargin;

        public int PostMargin
        {
            get { return _postMargin; }
            set { SetProperty(ref _postMargin, value); }
        }
        private int _antMargin;
        private IEventAggregator _eventAggregator;

        public int AntMargin
        {
            get { return _antMargin; }
            set { SetProperty(ref _antMargin, value); }

        }
        public bool bSave { get; set; }
        public SimpleStructureStepSource Source { get; set; }
        public int StepId { get; set; }
        public DelegateCommand SaveMarginCommand { get; private set; }
        public DelegateCommand CancelMarginCommand { get; private set; }
        public AsymmetricMarginViewModel(string currentMargin, SimpleStructureStepSource source, int stepId, IEventAggregator eventAggregator)
        {
            InterpretMargin(currentMargin);
            _eventAggregator = eventAggregator;
            Source = source;
            StepId = stepId;
            SaveMarginCommand = new DelegateCommand(OnSaveMargin);
            CancelMarginCommand = new DelegateCommand(OnCancelMargin);
        }

        private void OnCancelMargin()
        {
            _eventAggregator.GetEvent<SaveAsymmetricMarginEvent>().Publish(this);
        }

        private void OnSaveMargin()
        {
            bSave = true;
            _eventAggregator.GetEvent<SaveAsymmetricMarginEvent>().Publish(this);
        }

        private void InterpretMargin(string currentMargin)
        {
            if (!String.IsNullOrEmpty(currentMargin))
            {
                if (currentMargin.Contains("^"))//parse margins for default
                {
                    LeftMargin = Convert.ToInt16(currentMargin.Split('^').First());
                    RightMargin = Convert.ToInt16(currentMargin.Split('^').ElementAt(1));
                    SupMargin = Convert.ToInt16(currentMargin.Split('^').ElementAt(2));
                    InfMargin = Convert.ToInt16(currentMargin.Split('^').ElementAt(3));
                    PostMargin = Convert.ToInt16(currentMargin.Split('^').ElementAt(4));
                    AntMargin = Convert.ToInt16(currentMargin.Split('^').Last());
                }
                else//put all defaults into same margin
                {
                    LeftMargin = RightMargin = SupMargin = InfMargin = PostMargin = AntMargin = Convert.ToInt16(currentMargin);
                }
            }
        }
    }
}
