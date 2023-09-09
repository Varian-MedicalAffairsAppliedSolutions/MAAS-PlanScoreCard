using MahApps.Metro.Controls;
using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Models;
using PlanScoreCard.Services;
using PlanScoreCard.ViewModels.VMHelpers;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace PlanScoreCard.Views.HelperWindows
{
    /// <summary>
    /// Interaction logic for StructureDictionarySelectorView.xaml
    /// </summary>
    public partial class StructureDictionarySelectorView : MetroWindow
    {
        public StructureDictionarySelectorView(string newStructureId, string templateStructureId, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            StructureDictionarySelectorViewModel SSVM = new StructureDictionarySelectorViewModel(newStructureId, templateStructureId, eventAggregator);
            this.DataContext = SSVM;
            if(SSVM.CloseAction == null)
            {
                SSVM.CloseAction = new Action(this.Close);
            }

        }

    }
}
