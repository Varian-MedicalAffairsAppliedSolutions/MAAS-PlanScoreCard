using Microsoft.Win32;
using ScorecardVisualizer.Models;

namespace ScorecardVisualizer.Services.Commands
{
    internal class LaunchOpenScorecardPromptCommand : CommandBase
    {
        private ScorecardModel _scorecardModel;

        public override void Execute(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "JSON template (*.json)|*.json";
            ofd.Title = "Open Plan Scorecard";

            if (ofd.ShowDialog() == true)
            {
                if (_scorecardModel.OpenScorecard(ofd.FileName))
                    Messenger.SendUpdateScorecard();
            }
        }

        public LaunchOpenScorecardPromptCommand(ScorecardModel scorecardModel)
        {
            _scorecardModel = scorecardModel;
        }
    }
}
