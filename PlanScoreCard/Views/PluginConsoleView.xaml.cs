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
    /// Interaction logic for PluginConsoleView.xaml
    /// </summary>
    public partial class PluginConsoleView : UserControl
    {
        PluginConsoleViewModel PluginConsoleViewModel;

        public PluginConsoleView(PluginConsoleViewModel pluginConsoleViewModel)
        {
            PluginConsoleViewModel = pluginConsoleViewModel;
            DataContext = pluginConsoleViewModel; 
            InitializeComponent();
        }
    }
}
