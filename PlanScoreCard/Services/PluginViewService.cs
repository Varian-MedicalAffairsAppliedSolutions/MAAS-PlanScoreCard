using PlanScoreCard.Views;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using OxyPlot;

namespace PlanScoreCard.Services
{
    public class PluginViewService
    {
        private readonly IEventAggregator EventAggregator;
        private readonly IEventAggregator ViewEventAggregator;

        private PluginView View;
        private Dispatcher ViewDispatcher;

        public PluginViewService(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            ViewEventAggregator = new EventAggregator();
        }

        public bool ShowPluginView()
        {
            if (View == null)
            {
                Thread viewThread = new Thread(new ThreadStart(() =>
                {
                    ViewDispatcher = Dispatcher.CurrentDispatcher;
                    View = new PluginView();
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

        public bool UpdatePlot(PlotModel plotData)
        {

            try
            {
                if (View == null)
                    return false;

                ViewDispatcher.BeginInvoke((Action)(() =>
                {
                    View.UpdatePlot(plotData);
                    View.UpdateLayout();
                }));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateConsoleOutput(string consoleOutput)
        {

            try
            {
                if (View == null)
                    return false;

                ViewDispatcher.BeginInvoke((Action)(() =>
                {
                    View.UpdateConsoleOutput(consoleOutput);
                    View.UpdateLayout();
                }));

                return true;
            }
            catch
            {
                return false;
            }
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
