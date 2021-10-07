using PlanScoreCard.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace PlanScoreCard.Views
{
    /// <summary>
    /// Interaction logic for StructureGenerationVIew.xaml
    /// </summary>
    public partial class StructureGenerationVIew : Window
    {
        private GenerateScorecardViewModel GenerateScorecardViewModel;

        public StructureGenerationVIew(GenerateScorecardViewModel generateScorecardViewModel)
        {
            GenerateScorecardViewModel = generateScorecardViewModel;
            DataContext = GenerateScorecardViewModel;
            InitializeComponent();
        }
    }
}
