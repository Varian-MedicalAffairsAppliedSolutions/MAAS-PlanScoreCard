﻿using PlanScoreCard.ViewModels.MetricEditors;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlanScoreCard.Views.MetricEditors
{
    /// <summary>
    /// Interaction logic for EditDoseAtVolumeView.xaml
    /// </summary>
    public partial class EditDoseAtVolumeView : UserControl
    {

        private EditDoseAtVolumeViewModel EditDoseAtVolumeViewModel;

        public EditDoseAtVolumeView(EditDoseAtVolumeViewModel editDoseAtVolumeViewModel)
        {
            EditDoseAtVolumeViewModel = editDoseAtVolumeViewModel;
            DataContext = EditDoseAtVolumeViewModel; 
            InitializeComponent();
        }
        public EditDoseAtVolumeView()
        {
            InitializeComponent();
        }
    }
}
