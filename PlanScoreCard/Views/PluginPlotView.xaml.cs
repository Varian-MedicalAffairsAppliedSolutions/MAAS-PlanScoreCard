﻿using PlanScoreCard.ViewModels;
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

namespace PlanScoreCard.Views.Plugins
{
    /// <summary>
    /// Interaction logic for PluginPlotView.xaml
    /// </summary>
    public partial class PluginPlotView : UserControl
    {
        PluginPlotViewModel PluginPlotViewModel;

        public PluginPlotView(PluginPlotViewModel pluginPlotViewModel)
        {
            PluginPlotViewModel = pluginPlotViewModel; 
            DataContext = PluginPlotViewModel;
            InitializeComponent();
        }
    }
}
