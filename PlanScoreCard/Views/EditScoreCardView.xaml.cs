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
    /// Interaction logic for EditScoreCardView.xaml
    /// </summary>
    public partial class EditScoreCardView : MetroWindow
    {

        private EditScoreCardViewModel EditScoreCardViewModel;

        public EditScoreCardView(EditScoreCardViewModel editScoreCardViewModel)
        {
            EditScoreCardViewModel = editScoreCardViewModel;
            DataContext = EditScoreCardViewModel;
            InitializeComponent();
        }
    }
}
