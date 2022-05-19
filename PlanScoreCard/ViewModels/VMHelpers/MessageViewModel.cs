using PlanScoreCard.Events.HelperWindows;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels.VMHelpers
{
    public class MessageViewModel
    {
        public string Message { get; set; }
        public string MessageTitle { get; set; }

        private IEventAggregator _eventAggregator;
        public DelegateCommand CloseWindowCommand { get; private set; }
        public MessageViewModel(string messageTitle, string message, IEventAggregator eventAggregator)
        {
            Message = message; 
            MessageTitle = messageTitle;
            _eventAggregator = eventAggregator;
            CloseWindowCommand = new DelegateCommand(OnClose);
        }

        private void OnClose()
        {
            _eventAggregator.GetEvent<CloseMessageViewEvent>().Publish();
        }
    }
}
