using Prism.Mvvm;

namespace PlanScoreCard.Models
{
    public class OperationModel : BindableBase
	{
		private string _selectedOperationTxt;

		public string SelectedOperationTxt
		{
			get { return _selectedOperationTxt; }
			set { SetProperty(ref _selectedOperationTxt, value); }
		}

		private StructureOperationEnum _selectedOperationEnum;

		public StructureOperationEnum SelectedOperationEnum
		{
			get { return _selectedOperationEnum; }
			set { SetProperty(ref _selectedOperationEnum, value); }
		}
		private string _selectedOperationChar;

		public string SelectedOperationChar
		{
			get { return _selectedOperationChar; }
			set { SetProperty(ref _selectedOperationChar, value); }
		}


		public OperationModel(string operationTxt, StructureOperationEnum operationEnum, string operationChar)
		{
			SelectedOperationTxt = operationTxt;
			SelectedOperationEnum = operationEnum;
			SelectedOperationChar = operationChar;
		}
	}
}
