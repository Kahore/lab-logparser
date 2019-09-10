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
using System.Windows.Input;
using System.Windows.Threading;
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

		public string ReadableLogNameJSON { get; set; }

		public ObservableCollection<ReadableLogViewModel> ReadableLogs { get => _readableLogs; set { _readableLogs = value; NotifyPropertyChanged("ReadableLogs"); } }

		public ReadableLogViewModel ReadableLog { get => _readableLog; set { _readableLog = value; NotifyPropertyChanged("ReadableLog"); } }

		private AllJSONModelViewModel _allJSON;

		private ObservableCollection<AllJSONModelViewModel> _allJSONs;

		public ObservableCollection<AllJSONModelViewModel> AllJSONs { get => _allJSONs; set { _allJSONs = value; NotifyPropertyChanged("AllJSONs"); } }

		public AllJSONModelViewModel AllJSON { get => _allJSON; set { _allJSON = value; NotifyPropertyChanged("AllJSON"); } }

		public ViewModelMain()
		{
			ReadableLogs = new ObservableCollection<ReadableLogViewModel>();
			AllJSONs = new ObservableCollection<AllJSONModelViewModel>();
			ReadableLogs.CollectionChanged += new NotifyCollectionChangedEventHandler(ReadableLogs_CollectionChanged);
			pathToWatch = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\Export\";
			logFile = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\log.txt";
			GetMeAllJSON();
			RunWatch();
		}

		private void GetMeAllJSON()
		{
			var allFilenames = Directory.EnumerateFiles(Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\").Select(p => Path.GetFileName(p));

			var candidates = allFilenames.Where(fn => Path.GetExtension(fn) == ".json")
							 .Select(fn => Path.GetFileNameWithoutExtension(fn));
			foreach (var item in candidates)
			{
				App.Current.Dispatcher.Invoke((Action)delegate
				{
					bool IsListContainsUserName = AllJSONs.Any(x => x.JSONName == item);
					if (!IsListContainsUserName)
					{
						AllJSON = new AllJSONModelViewModel(item);
						AllJSONs.Add(AllJSON);
					}
				});	
			}
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
			//Thread.Sleep(2000);
			try
			{
				if (e.ChangeType == WatcherChangeTypes.Created)
				{
					ReadableLogNameJSON = e.Name.Substring(0,e.Name.Length-4);
					StreamReader myReader = new StreamReader(e.FullPath, Encoding.GetEncoding(1251));
					var filePath = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\" + ReadableLogNameJSON + ".json";
					while (true)
					{
						// Читаем строку из файла во временную переменную.
						string temp = myReader.ReadLine();
						if (temp == null) break;
						string[] words = temp.Split(';');
						var lol = words[9];
						ReadableLog = new ReadableLogViewModel(Guid.NewGuid(), words[6], words[3], words[7], words[8], words[1], words[9], words[0]);

						if (!File.Exists(filePath))
						{
							var newTestOperator = "[{\"UserName\":\"TestUser\",\"Id\":\"" + Guid.NewGuid() + "\"}]";
							File.WriteAllText(filePath, newTestOperator);
						}

							string json = File.ReadAllText(filePath);
						//TODO somefthing
						var list = JsonConvert.DeserializeObject<List<ReadableLogViewModel>>(json);

							bool IsListContainsUserName = list.Any(x => x.UserName == _readableLog.UserName && x.DateTimeCorrectedAction == _readableLog.DateTimeCorrectedAction);
							if (!IsListContainsUserName)
							{
								list.Add(new ReadableLogViewModel(_readableLog));
								var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);
								File.WriteAllText(filePath, convertedJson);
							}

						// Если достигнут конец файла, прерываем считывание.
					
					}

					myReader.Close();
					myReader.Dispose();

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
				//throw new System.NullReferenceException();
			}
			finally
			{
				var filePath = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\" + ReadableLogNameJSON + ".json";	
			}
			GetMeAllJSON();

			//	Thread.Sleep(1000);
		}

		#region ICommand _selectJSONCommand;
		private ICommand _selectJSONCommand;
		public ICommand SelectJSONCommand
		{
			get
			{
				if (_selectJSONCommand == null)
				{
					_selectJSONCommand = new ActionCommand(param => RefreshAll(AllJSON.JSONName),
						null);
				}
				return _selectJSONCommand;
			}
		}

		void RefreshAll(string readableLogNameJSON)
		{
			ReadableLogs.Clear();
			var filePath = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\"+readableLogNameJSON + ".json";
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
		}
		#endregion
	}
}
