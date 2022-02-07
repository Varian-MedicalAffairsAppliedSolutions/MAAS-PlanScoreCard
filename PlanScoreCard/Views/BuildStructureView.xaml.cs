using MahApps.Metro.Controls;
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
    /// Interaction logic for BuildStructureView.xaml
    /// </summary>
    public partial class BuildStructureView : MetroWindow
    {
        private BuildStructureViewModel BuildStructureViewModel {get;set;}
        public BuildStructureView(BuildStructureViewModel buildStructureViewModel)
        {
            BuildStructureViewModel = buildStructureViewModel;
            DataContext = BuildStructureViewModel;
            InitializeComponent();

        }
    }
}
