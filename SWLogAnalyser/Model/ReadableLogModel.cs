using System;

namespace SWLogAnalyser.Model
{
	public class ReadableLogModel
	{
		public Guid Id { get; set; }
		public string Field { get; set; }
		public string OldValue { get; set; }
		public string CorrectedValue { get; set; }
		public string UserName { get; set; }
		public string Method { get; set; }
		public DateTime Date { get; set; }
	}
}
