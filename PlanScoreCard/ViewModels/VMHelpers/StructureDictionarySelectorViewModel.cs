using Microsoft.Win32;
using PlanScoreCard.Events.HelperWindows;
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
    public class StructureDictionarySelectorViewModel : BindableBase
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
            set { SetProperty(ref _structureToAdd, value); }
        }

        private string _templateStructureId;
        private bool _bAddToDictionary;

        public bool bAddToDictionary
        {
            get { return _bAddToDictionary; }
            set
            {
                SetProperty(ref _bAddToDictionary, value);
                if (bAddToDictionary)
                {
                    bAddNewEntry = false;
                    bDeleteStructureDictionaryEntry = false;
                    bMergeDictionaryFromFile = false;
                }
            }
        }
        private bool _bAddNewEntry;

        public bool bAddNewEntry
        {
            get { return _bAddNewEntry; }
            set
            {
                SetProperty(ref _bAddNewEntry, value);
                if (bAddNewEntry)
                {
                    SelectedDictionaryKey = null;
                    bAddToDictionary = false;
                    bDeleteStructureDictionaryEntry = false;
                    bMergeDictionaryFromFile = false;
                }
            }
        }
        private bool _bDeleteStructureDictionaryEntry;

        public bool bDeleteStructureDictionaryEntry
        {
            get { return _bDeleteStructureDictionaryEntry; }
            set
            {
                SetProperty(ref _bDeleteStructureDictionaryEntry, value);
                if (bDeleteStructureDictionaryEntry)
                {
                    SelectedDictionaryKey = null;
                    bAddNewEntry = false;
                    bAddToDictionary = false;
                    bMergeDictionaryFromFile = false;
                }
            }
        }
        private bool _bMergeDictionaryFromFile;

        public bool bMergeDictionaryFromFile
        {
            get { return _bMergeDictionaryFromFile; }
            set { SetProperty(ref _bMergeDictionaryFromFile,value);
                if (bMergeDictionaryFromFile)
                {
                    bAddNewEntry = false;
                    bAddToDictionary = false;
                    bDeleteStructureDictionaryEntry = false;
                }
            }
        }

        private string _entryKey;

        public string EntryKey
        {
            get { return _entryKey; }
            set { SetProperty(ref _entryKey, value); }
        }
        private string _fileToMergeText;

        public string FileToMergeText
        {
            get { return _fileToMergeText; }
            set { SetProperty(ref _fileToMergeText, value); }
        }


        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand UpdateDictionaryCommand { get; private set; }
        public Action CloseAction { get; set; }
        public DelegateCommand OpenDictionaryFileCommand { get; private set; }
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
            OpenDictionaryFileCommand = new DelegateCommand(OnOpenDictionaryFile);
            //CloseAction = new Action(OnClose);
            Bind();
        }

        private void OnOpenDictionaryFile()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "JSON(*.json)|*.json";
            ofd.Title = "Open Structure Dictionary File to Merge";
            if(ofd.ShowDialog() == true)
            {
                FileToMergeText = ofd.FileName; 
            }
        }

        private void OnClose()
        {
            //closes the window
        }

        private void OnDictionaryUpdate()
        {
            if (bAddToDictionary)
            {
                if (StructureDictionaryService.AddSynonym(SelectedDictionaryKey, StructureToAdd))
                {
                    EventAggregator.GetEvent<StructureDictionaryAddedEvent>().Publish();
                }
            }
            if (bAddNewEntry)
            {
                StructureDictionaryService.AddStructure(EntryKey);
                if (StructureDictionaryService.AddSynonym(EntryKey, StructureToAdd))
                {
                    EventAggregator.GetEvent<StructureDictionaryAddedEvent>().Publish();
                }

            }
            if (bDeleteStructureDictionaryEntry)
            {
                if(SelectedDictionaryKey != null)
                {
                    StructureDictionaryService.DeleteStructure(SelectedDictionaryKey);
                }
            }
            if (bMergeDictionaryFromFile)
            {
                if (!String.IsNullOrEmpty(FileToMergeText))
                {
                    StructureDictionaryService.MergeDictionary(FileToMergeText);
                }
                else
                {
                    System.Windows.MessageBox.Show("No Merge File Selected");
                }
            }
            CloseAction.Invoke();
        }

        private void OnCancel()
        {
            //throw new NotImplementedException();
            CloseAction.Invoke();
            //OnClose();
        }

        private void Bind()
        {
            foreach (StructureDictionaryModel structure in StructureDictionaryService.StructureDictionary)
            {
                DictionaryKeys.Add(structure.StructureID);
            }
            if (DictionaryKeys.Any(x => x.Equals(_templateStructureId, StringComparison.OrdinalIgnoreCase)))
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
