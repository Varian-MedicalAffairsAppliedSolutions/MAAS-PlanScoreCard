using ScorecardVisualizer.Models;
using System.Windows.Controls;

namespace ScorecardVisualizer.Services.Commands
{
    internal class FocusOnStructureCommand : CommandBase
    {
        private ScorecardModel _scorecardModel;

        public override void Execute(object parameter)
        {
            var values = (object[])parameter;

            string structureId = values[0] as string;
            Grid grid = values[1] as Grid;

            ListView metricDisplayListView = null;
            ListView legendListView = null;

            foreach (var child1 in grid.Children)
            {
                if (child1 is Border border)
                {
                    if (border.Child is UserControl userControl)
                    {
                        if (userControl.Content is ListView listView)
                        {
                            if (listView.Name == "MetricDisplayListView")
                            {
                                metricDisplayListView = listView;
                            }
                            else if (listView.Name == "LegendListView")
                            {
                                legendListView = listView;
                            }
                        }
                    }
                }
            }

            StructurePlotInfo selectedStructurePlotInfo = _scorecardModel.LoadPlotModel(structureId);

            Messenger.SendUpdatePlot();
            Messenger.SendSelectStructure(selectedStructurePlotInfo, metricDisplayListView, legendListView);
        }

        public FocusOnStructureCommand(ScorecardModel scorecardModel)
        {
            _scorecardModel = scorecardModel;
        }
    }
}
