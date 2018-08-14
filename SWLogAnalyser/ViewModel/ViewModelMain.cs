using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
/*================================================*/
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SWLogAnalyser.Command;

namespace SWLogAnalyser.ViewModel
{
	public class ViewModelMain : ViewModelBase
	{
		private readonly string pathToWatch;
		private readonly string logFile;
		private ReadableLogViewModel _readableLog;
		private string fileText;
		private const string FileToWatch = "*.log";

		private ObservableCollection<ReadableLogViewModel> _readableLogs;

		public string FileText
		{
			get => fileText;
			set
			{
				if (fileText == value) return;
				fileText = value;
				NotifyPropertyChanged("FileText");
			}
		}
		public ObservableCollection<ReadableLogViewModel> ReadableLogs { get => _readableLogs; set { _readableLogs = value; NotifyPropertyChanged("ReadableLogs"); } }

		public ReadableLogViewModel ReadableLog { get => _readableLog; set { _readableLog = value; NotifyPropertyChanged("ReadableLog"); } }

		public ViewModelMain()
		{
			ReadableLogs = new ObservableCollection<ReadableLogViewModel>();
			ReadableLogs.CollectionChanged += new NotifyCollectionChangedEventHandler(ReadableLogs_CollectionChanged);
			pathToWatch = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\Export\";
			logFile = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\log.txt";
			RunWatch();
			RefreshAll();
		}

		void ReadableLogs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyPropertyChanged("ReadableLogs");
		}
		public void RunWatch()
		{
			var watcher = new FileSystemWatcher
			{
				NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime,

				Path = pathToWatch,
				Filter = FileToWatch
			};
			watcher.Created += OnChanged;
			watcher.EnableRaisingEvents = true;
		}
		private void OnChanged(object source, FileSystemEventArgs e)
		{
			Thread.Sleep(2000);
			try
			{
				if (e.ChangeType == WatcherChangeTypes.Created)
				{

					StreamReader myReader = new StreamReader(e.FullPath, Encoding.GetEncoding(1251));
					while (true)
					{
						// Читаем строку из файла во временную переменную.
						string temp = myReader.ReadLine();
						string[] words = temp.Split(';');
						var lol = words[9];
						ReadableLog = new ReadableLogViewModel(Guid.NewGuid(), words[6], words[3], words[7], words[8], words[1], words[9], words[0]);
						//Guid id, string Field, string LabNo, string OldValue, string CorrectedValue, 
						//string UserName, string Method, string DateTimeCorrectedAction
						var filePath = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\ReadableLog.json";

						if (!File.Exists(filePath))
						{
							var newTestOperator = "[{\"UserName\":\"TestUser\",\"Id\":\"" + Guid.NewGuid() + "\"}]";
							File.WriteAllText(filePath, newTestOperator);
							using (StreamWriter file = File.CreateText(filePath))
							{
								JsonSerializer serializer = new JsonSerializer();
								serializer.Serialize(file, ReadableLog);
							}
						}
						else
						{
							string json = File.ReadAllText(filePath);

							var list = JsonConvert.DeserializeObject<List<ReadableLogViewModel>>(json);

							bool IsListContainsUserName = list.Any(x => x.UserName == _readableLog.UserName && x.DateTimeCorrectedAction == _readableLog.DateTimeCorrectedAction);
							if (!IsListContainsUserName)
							{
								list.Add(new ReadableLogViewModel(_readableLog));
								var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);
								File.WriteAllText(filePath, convertedJson);
							}
						}
						ReadableLogs.Add(ReadableLog);

						// Если достигнут конец файла, прерываем считывание.
						if (temp == null) break;
					}

				myReader.Close();
					myReader.Dispose();

					StreamWriter myWriter = new StreamWriter(e.FullPath);

					//перед чтением необходимо вернуть указатель потока в нужное место, у меня - в начало файла.
					//myWriter.Flush();
					myWriter.Close();
					myWriter.Dispose();

				//	File.Move(e.FullPath, dest + e.Name);

					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();
				}
			}
			catch (FileNotFoundException ex)
			{
				FileText = "File not found.";
				File.AppendAllText(logFile, FileText + ' ' + e.Name + ' ' + ex.ToString());

			}
			catch (FileLoadException ex)
			{
				FileText = "File Failed to load";
				File.AppendAllText(logFile, FileText + ' ' + e.Name + ' ' + ex.ToString());
			}
			catch (IOException ex)
			{
				FileText = "File I/O Error";
				File.AppendAllText(logFile, FileText + ' ' + e.Name + ' ' + ex.ToString());
			}
			catch (Exception err)
			{
				FileText = err.Message;
				File.AppendAllText(logFile, FileText + ' ' + e.Name + ' ' + err.ToString());
			}
			Thread.Sleep(1000);
		}
		void RefreshAll()
		{
			ReadableLogs.Clear();
			

			var filePath = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\ReadableLog.json";
			if (File.Exists(filePath))
			{
				using (StreamReader r = new StreamReader(filePath))
				{
					string jsonData = File.ReadAllText(filePath);

					var list = JsonConvert.DeserializeObject<List<ReadableLogViewModel>>(jsonData);
					var descListOb = list.OrderBy(x => x.Field).ThenBy(x => x.UserName).ThenBy(x => x.LabNo).ThenBy(x => x.DateTimeCorrectedAction);
					descListOb.ToList().ForEach(ReadableLogs.Add);
				}
			}
			else
			{
				var newTestOperator = "[{\"UserName\":\"TestUser\",\"Id\":\"" + Guid.NewGuid() + "\"}]";
				File.WriteAllText(filePath, newTestOperator);
			}
		}
	}
}
