using System;
using System.Collections.Generic;
/*------------------------------------------------*/
using Newtonsoft.Json;
using SWLogAnalyser.Model;

namespace SWLogAnalyser.ViewModel
{
	public class ReadableLogViewModel : ViewModelBase
	{
		ReadableLogModel _readableLogModel;

		private List<ReadableLogViewModel> _list;

		public ReadableLogViewModel(ReadableLogModel readableLogModel) => _readableLogModel = readableLogModel;

		public ReadableLogViewModel(ReadableLogViewModel vmReadableLogViewModel) => 
			_readableLogModel = new ReadableLogModel { Id = vmReadableLogViewModel.Id, Field=vmReadableLogViewModel.Field,
				LabNo=vmReadableLogViewModel.LabNo, OldValue =vmReadableLogViewModel.OldValue,
				CorrectedValue=vmReadableLogViewModel.CorrectedValue, UserName = vmReadableLogViewModel.UserName,
				Method= vmReadableLogViewModel.Method, DateTimeCorrectedAction=vmReadableLogViewModel.DateTimeCorrectedAction
			};

		public ReadableLogViewModel(List<ReadableLogViewModel> list) => _list = list;

		[JsonConstructor]
		public ReadableLogViewModel(Guid id, string Field, string LabNo, string OldValue, string CorrectedValue, 
									string UserName, string Method, string DateTimeCorrectedAction) => 
			_readableLogModel =	new ReadableLogModel { Id = id, Field = Field,
				LabNo = LabNo, OldValue = OldValue, CorrectedValue = CorrectedValue,
														UserName = UserName, Method = Method, DateTimeCorrectedAction = DateTimeCorrectedAction };

		public Guid Id { get=> _readableLogModel.Id; set=> _readableLogModel.Id=value; }

		public string Field { get => _readableLogModel.Field; set => _readableLogModel.Field = value; }

		public string LabNo { get => _readableLogModel.LabNo; set => _readableLogModel.LabNo = value; }

		public string OldValue { get => _readableLogModel.OldValue; set => _readableLogModel.OldValue = value; }

		public string CorrectedValue { get => _readableLogModel.CorrectedValue; set => _readableLogModel.CorrectedValue = value; }

		public string UserName { get => _readableLogModel.UserName; set => _readableLogModel.UserName = value; }

		public string Method { get => _readableLogModel.Method; set => _readableLogModel.Method = value; }

		public string DateTimeCorrectedAction { get => _readableLogModel.DateTimeCorrectedAction; set => _readableLogModel.DateTimeCorrectedAction = value; }
	}
}
