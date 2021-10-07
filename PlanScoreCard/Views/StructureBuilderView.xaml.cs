using MahApps.Metro.Controls;
using PlanScoreCard.ViewModels;

namespace PlanScoreCard.Views
{
    /// <summary>
    /// Interaction logic for StructureBuilderView.xaml
    /// </summary>
    public partial class StructureBuilderView : MetroWindow
    {

        public StructureBuilderViewModel StructureBuilderViewModel;

        public StructureBuilderView(StructureBuilderViewModel structureBuilderViewModel)
        {
            StructureBuilderViewModel = structureBuilderViewModel;
            DataContext = StructureBuilderViewModel; 
            InitializeComponent();
        }
    }
}
