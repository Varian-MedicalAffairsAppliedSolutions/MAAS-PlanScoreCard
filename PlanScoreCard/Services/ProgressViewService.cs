using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Views.HelperWindows;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PlanScoreCard.Services
{
    public class ProgressViewService
    {
        private readonly IEventAggregator EventAggregator;
        private readonly IEventAggregator ViewEventAggregator;

        private ProgressView View;
        private Dispatcher ViewDispatcher;

        public ProgressViewService(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            ViewEventAggregator = new EventAggregator();

            ViewEventAggregator.GetEvent<CancelEvent>().Subscribe(Canceled);
        }

        public bool ChangeLoop(bool loop)
        {
            if (View != null)
            {
                ViewDispatcher.BeginInvoke((Action)(() =>
                {
                    View.Loop = loop;
                    View.UpdateLayout();
                }));
                return true;
            }

            return false;
        }

        public bool ChangeMessage(string message)
        {
            if (View != null)
            {
                ViewDispatcher.BeginInvoke((Action)(() =>
                {
                    View.Message = message;
                    View.UpdateLayout();
                }));
                return true;
            }

            return false;
        }

        public bool ChangeProgress(double progress)
        {
            if (View != null)
            {
                ViewDispatcher.BeginInvoke((Action)(() =>
                {
                    View.Progress = progress;
                    View.UpdateLayout();
                }));
                return true;
            }

            return false;
        }

        public bool Close()
        {
            if (View != null)
            {
                ViewDispatcher.BeginInvoke((Action)(() =>
                {
                    View.Close();
                }));

                return true;
            }

            return false;
        }

        public bool ShowProgress(string message, double progress, bool loop, bool cancel = false)
        {
            if (View == null)
            {
                Thread viewThread = new Thread(new ThreadStart(() =>
                {
                    ViewDispatcher = Dispatcher.CurrentDispatcher;
                    View = new ProgressView(message, progress, loop, cancel, ViewEventAggregator);
                    View.Closed += WindowClosed;
                    View.ShowDialog();
                }))
                { IsBackground = true };
                viewThread.SetApartmentState(ApartmentState.STA);
                viewThread.Start();

                return true;
            }
            return false;
        }

        private void Canceled()
        {
            EventAggregator.GetEvent<CancelEvent>().Publish();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            View = null;
            ViewDispatcher = null;
        }

        public bool SendToFront()
        {
            if (View != null)
            {
                ViewDispatcher.BeginInvoke((Action)(() =>
                {
                    View.SendToFront();
                    View.UpdateLayout();
                }));
                return true;
            }

            return false;
        }

    }
}
