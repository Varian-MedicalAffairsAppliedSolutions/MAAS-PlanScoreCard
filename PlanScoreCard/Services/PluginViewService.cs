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
        // Event Aggregator
        private readonly IEventAggregator EventAggregator;

        // PluginView and Dispatcher
        private PluginView View;
        private Dispatcher ViewDispatcher;

        // PluginViewService Constructor
        public PluginViewService(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
        }

        // This gets the thread that the fiew is on. Declares the local variable ViewDispatcher.
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

        // Updates the Console's Output
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

        // Sends the Plugin to the Front
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

        // Sends the PlotModel data through to the View.
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

        // This adds a point each time it is called
        internal bool AddPlotPoint(double xval, double yval)
        {
            try
            {
                if (View == null)
                    return false;

                ViewDispatcher.BeginInvoke((Action)(() =>
                {
                    View.AddPlotPoint(xval, yval);
                    View.UpdateLayout();
                }));

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Update the X and Y Axis
        internal bool UpdateXAxisLebel(string xAxisLabel)
        {
            try
            {
                if (View == null)
                    return false;

                ViewDispatcher.BeginInvoke((Action)(() =>
                {
                    View.SetXAxisLabel(xAxisLabel);
                    View.UpdateLayout();
                }));

                return true;
            }
            catch
            {
                return false;
            }
        }

        internal bool UpdateYAxisLebel(string yAxisLabel)
        {
            try
            {
                if (View == null)
                    return false;

                ViewDispatcher.BeginInvoke((Action)(() =>
                {
                    View.SetYAxisLabel(yAxisLabel);
                    View.UpdateLayout();
                }));

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Close Method
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

        // WindowClosed Method
        private void WindowClosed(object sender, EventArgs e)
        {
            View = null;
            ViewDispatcher = null;
        }
    }
}
