using MahApps.Metro.Controls;
using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Models;
using PlanScoreCard.Services;
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
    public partial class StructureDictionarySelectorView : MetroWindow, INotifyPropertyChanged
    {

        private readonly IEventAggregator EventAggregator;

        private StructureDictionaryService StructureDictionaryService;

        private string NewStructureId;

        private List<string> dictionaryStructures;

        public List<string> DictionaryStructures
        {
            get { return dictionaryStructures; }
            set
            {
                if (dictionaryStructures != value)
                {
                    dictionaryStructures = value;
                    OnPropertyChanged();
                }
            }
        }

        private string selectedDictionaryStructure;

        public string SelectedDictionaryStructure
        {
            get { return selectedDictionaryStructure; }
            set
            {
                if (selectedDictionaryStructure != value)
                {
                    selectedDictionaryStructure = value;
                    OnPropertyChanged();
                }
            }
        }

        public StructureDictionarySelectorView(StructureDictionaryService structureDictionaryService, string newStructureId, IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            NewStructureId = newStructureId; 

            StructureDictionaryService = structureDictionaryService;
            DictionaryStructures = new List<string>();
            Bind();
        }

        private void Bind()
        {
            foreach (StructureDictionaryModel structure in StructureDictionaryService.StructureDictionary)
            {
                DictionaryStructures.Add(structure.StructureID);
            }

            DictionaryStructures.Add("'Add New Structure'");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDictionaryStructure == null)
            {
                MessageBox.Show("No Structure Selected. Please Select A Structure to Match to.");
                return;
            }
            else if (SelectedDictionaryStructure.Equals(DictionaryStructures.Last()))
            {
                MessageBoxResult result = MessageBox.Show("This structure is not a TG-263 structure, are you SURE you like to add it?", "New StructureId", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    StructureDictionaryService.AddStructure(NewStructureId);
                    Close();
                }
            }
            else
            {
                StructureDictionaryService.AddSynonym(SelectedDictionaryStructure, NewStructureId);
                Close();
            }

        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
