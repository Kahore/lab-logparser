using System.Collections.Generic;

using SWLogAnalyser.Model;

namespace SWLogAnalyser.ViewModel
{
	public class AllJSONModelViewModel : ViewModelBase
	{
		AllJSONModel _allJSONModel;

		private List<AllJSONModelViewModel> list;

		public AllJSONModelViewModel(AllJSONModel allJSONModel) => _allJSONModel = allJSONModel;

		public AllJSONModelViewModel(AllJSONModelViewModel vmAllJSONModelViewModel) => _allJSONModel = new AllJSONModel { JSONName = vmAllJSONModelViewModel.JSONName };

		public AllJSONModelViewModel(List<AllJSONModelViewModel> list) => this.list = list;

		public AllJSONModelViewModel(string jSONName) => _allJSONModel = new AllJSONModel { JSONName = jSONName };

		public string JSONName { get => _allJSONModel.JSONName; set { _allJSONModel.JSONName = value; NotifyPropertyChanged("JSONName"); } }
	}
}
