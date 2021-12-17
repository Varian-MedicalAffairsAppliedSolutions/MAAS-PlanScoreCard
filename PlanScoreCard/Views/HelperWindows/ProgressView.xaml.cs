using MahApps.Metro.Controls;
using PlanScoreCard.Events.HelperWindows;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PlanScoreCard.Views.HelperWindows
{
    /// <summary>
    /// Interaction logic for ProgressView.xaml
    /// </summary>
    public partial class ProgressView : MetroWindow, INotifyPropertyChanged
    {
        private readonly IEventAggregator EventAggregator;

        private string message;
        public string Message
        {
            get { return message; }
            set
            {
                if (message != value)
                {
                    message = value;
                    OnPropertyChanged();
                }
            }
        }

        private double progress;
        public double Progress
        {
            get { return progress; }
            set
            {
                if (progress != value)
                {
                    progress = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool loop;
        public bool Loop
        {
            get { return loop; }
            set
            {
                if (loop != value)
                {
                    loop = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool cancelVisible;
        public bool CancelVisible
        {
            get { return cancelVisible; }
            set
            {
                if (cancelVisible != value)
                {
                    cancelVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ProgressView(string initialMessage, double initialProgress, bool setLoop, bool showCancel, IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;

            Message = initialMessage;
            Progress = initialProgress;
            Loop = setLoop;
            CancelVisible = showCancel;

            DataContext = this;
            InitializeComponent();

            Topmost = true;
            Activate();
            Topmost = false;
        }

        public void SendToFront()
        {
            Topmost = true;
            Topmost = false;
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            EventAggregator.GetEvent<CancelEvent>().Publish();
        }
    }
}
