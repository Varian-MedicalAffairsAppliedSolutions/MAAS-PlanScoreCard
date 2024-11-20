using ScorecardVisualizer.Models;
using System;
using System.Windows.Controls;

namespace ScorecardVisualizer.Services
{
    public static class Messenger
    {
        public static event EventHandler UpdateScorecard;
        public static event EventHandler UpdatePlot;
        public static event EventHandler SelectStructure;

        public static void SendUpdateScorecard()
        {
            UpdateScorecard?.Invoke(null, EventArgs.Empty);
        }

        public static void SendUpdatePlot()
        {
            UpdatePlot?.Invoke(null, EventArgs.Empty);
        }

        public static void SendSelectStructure(StructurePlotInfo structurePlotInfo, ListView metricListView, ListView legendListView)
        {
            var parameters = new Tuple<StructurePlotInfo, ListView, ListView>(structurePlotInfo, metricListView, legendListView);

            SelectStructure?.Invoke(parameters, EventArgs.Empty);
        }
    }
}
