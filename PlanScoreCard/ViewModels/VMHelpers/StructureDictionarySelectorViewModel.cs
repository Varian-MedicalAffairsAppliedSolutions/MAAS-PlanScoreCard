using PlanScoreCard.Models;
using PlanScoreCard.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels.VMHelpers
{
    public class StructureDictionarySelectorViewModel:BindableBase
    {
        private readonly IEventAggregator EventAggregator;

        private StructureDictionaryService StructureDictionaryService;

        //private string NewStructureId;

        public ObservableCollection<string> DictionaryKeys { get; private set; }

        private string _selectedDictionaryKey;

        public string SelectedDictionaryKey
        {
            get { return _selectedDictionaryKey; }
            set
            {
                SetProperty(ref _selectedDictionaryKey, value);
            }
        }
        private string _structureToAdd;

        public string StructureToAdd
        {
            get { return _structureToAdd; }
            set { SetProperty(ref _structureToAdd,value); }
        }

        private string _templateStructureId;
        private bool _bAddToDictionary;

        public bool bAddToDictionary
        {
            get { return _bAddToDictionary; }
            set { 
                SetProperty(ref _bAddToDictionary,value);
                if (bAddToDictionary)
                {
                    bAddNewEntry = false;
                }
            }
        }
        private bool _bAddNewEntry;

        public bool bAddNewEntry
        {
            get { return _bAddNewEntry; }
            set { 
                SetProperty(ref _bAddNewEntry,value);
                if (bAddNewEntry)
                {
                    bAddToDictionary = false;
                }
            }
        }
        private string _entryKey;

        public string EntryKey
        {
            get { return _entryKey; }
            set { SetProperty(ref _entryKey,value); }
        }


        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand UpdateDictionaryCommand { get; private set; }
        public Action CloseAction { get; set; }
        public StructureDictionarySelectorViewModel(StructureDictionaryService structureDictionaryService, string newStructureId, string templateStructureId, IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            //NewStructureId = newStructureId;
            StructureToAdd = newStructureId;
            _templateStructureId = templateStructureId;
            StructureDictionaryService = structureDictionaryService;
            DictionaryKeys = new ObservableCollection<string>();
            //DictionaryStructures = new List<string>();
            CancelCommand = new DelegateCommand(OnCancel);
            UpdateDictionaryCommand = new DelegateCommand(OnDictionaryUpdate);
            Bind();
        }

        private void OnDictionaryUpdate()
        {
            if (bAddToDictionary)
            {
                StructureDictionaryService.AddSynonym(SelectedDictionaryKey, StructureToAdd);
            }
            if (bAddNewEntry)
            {
                StructureDictionaryService.AddStructure(SelectedDictionaryKey);
                StructureDictionaryService.AddSynonym(SelectedDictionaryKey, StructureToAdd);
            }
        }

        private void OnCancel()
        {
            throw new NotImplementedException();
        }

        private void Bind()
        {
            foreach (StructureDictionaryModel structure in StructureDictionaryService.StructureDictionary)
            {
                DictionaryKeys.Add(structure.StructureID);
            }
            if(DictionaryKeys.Any(x=>x.Equals(_templateStructureId, StringComparison.OrdinalIgnoreCase)))
            {
                bAddToDictionary = true;
                SelectedDictionaryKey = DictionaryKeys.FirstOrDefault(x => x.Equals(_templateStructureId, StringComparison.OrdinalIgnoreCase));
            }

            //DictionaryKeys.Add("'Add New Structure'");
        }

        //public event PropertyChangedEventHandler PropertyChanged;
        //private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        /*private void Button_Click(object sender, RoutedEventArgs e)
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
        }*/
    }
}
