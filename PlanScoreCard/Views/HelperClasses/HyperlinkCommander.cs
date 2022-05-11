using System.Windows.Documents;

namespace PlanScoreCard.Views.HelperClasses
{
    public class HyperlinkCommander : Hyperlink
    {
        protected override void OnClick()
        {
            System.Diagnostics.Process.Start(this.NavigateUri.ToString());
        }
    }
}
