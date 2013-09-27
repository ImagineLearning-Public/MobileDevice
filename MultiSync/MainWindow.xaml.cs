using MobileDevice;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MultiSync
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Queue<iPhoneSync> _devicesToSync;

		private string _sourceDirectory;
		private string _targetDirectory;
		private string _bundleIdentifier;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			SourceDirectory.Text = (string)Settings.Default["SourceDirectory"];
			TargetDirectory.Text = (string)Settings.Default["TargetDirectory"];
		}

		private void FindDevices_Click(object sender, RoutedEventArgs e)
		{
			Devices.Items.Clear();

			var deviceFinder = new DeviceFinder();
			deviceFinder.DeviceDiscovered += (s, args) => Dispatcher.BeginInvoke((Action)(() =>
			{
				Devices.Items.Add(
					new iPhoneSync
					{
						iPhone = args.iPhone
					});

				SyncDevices.IsEnabled = true;
			}));
			deviceFinder.GetiPhonesAsync();
		}

		private void SyncDevices_Click(object sender, RoutedEventArgs e)
		{
			SyncDevices.IsEnabled = false;

			_devicesToSync = new Queue<iPhoneSync>();
			foreach (var iPhoneSync in Devices.Items.Cast<iPhoneSync>())
			{
				_devicesToSync.Enqueue(iPhoneSync);
			}

			_sourceDirectory = SourceDirectory.Text;
			_targetDirectory = TargetDirectory.Text;
			_bundleIdentifier = (string)Settings.Default["BundleIdentifier"];

			BeginSync();
		}

		private void BeginSync()
		{
			var syncWorker = new BackgroundWorker();
			syncWorker.DoWork += (s, args) =>
			{
				var iPhoneSyncClosure = _devicesToSync.Dequeue();
				iPhoneSyncClosure.ProgressChanged +=
					(sender, progressChangedEventArgs) => syncWorker.ReportProgress(progressChangedEventArgs.NewProgress, iPhoneSyncClosure);
				iPhoneSyncClosure.SyncCompleted += (syncer, syncArgs) =>
				{
					if (_devicesToSync.Count > 0)
					{
						BeginSync();
					}
					else
					{
						Dispatcher.BeginInvoke((Action)(() =>
						{
							SyncDevices.IsEnabled = true;
							Devices.Items.Clear();
						}));
					}
				};
				var iPhoneClosure = iPhoneSyncClosure.iPhone;
				iPhoneClosure.ConnectViaHouseArrest(_bundleIdentifier);
				iPhoneSyncClosure.Sync(_sourceDirectory, _targetDirectory);
			};
			syncWorker.WorkerReportsProgress = true;
			syncWorker.ProgressChanged += (s, args) =>
			{
				((iPhoneSync)args.UserState).Progress = args.ProgressPercentage;
			};
			syncWorker.RunWorkerAsync();
		}
	}

	public class iPhoneSync : INotifyPropertyChanged
	{
		public iPhone iPhone { get; set; }
		private int _progress;
		public int Progress
		{
			get { return _progress; }
			set
			{
				_progress = value;
				OnPropertyChanged("Progress");
			}
		}

		public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
		public event EventHandler SyncCompleted;
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Sync(string sourceDirectory, string targetDirectory)
		{
			var root = new DirectoryInfo(sourceDirectory).Name;
			var files = Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.AllDirectories).ToList();
			for (int i = 0; i < files.Count; i++)
			{
				var file = files[i];
				var remoteFolder = Path.Combine(targetDirectory, new FileInfo(file).DirectoryName.Substring(file.IndexOf(root) + root.Length + 1)).Replace(@"\", "/");
				if (!iPhone.CreateDirectory(remoteFolder))
				{
					MessageBox.Show(string.Format("Create directory failed: {0}", sourceDirectory));
				}

				if (ProgressChanged != null)
				{
					ProgressChanged(this, new ProgressChangedEventArgs() { NewProgress = (int)(((double)i / (double)files.Count) * 100.0) });
				}

				iPhone.CopyFile(file, Path.Combine(remoteFolder, Path.GetFileName(file)).Replace(@"\", "/"));
			}

			if (ProgressChanged != null)
			{
				ProgressChanged(this, new ProgressChangedEventArgs() { NewProgress = 100 });
			}

			if (SyncCompleted != null)
			{
				SyncCompleted(this, new EventArgs());
			}
		}
	}

	public class ProgressChangedEventArgs : EventArgs
	{
		public int NewProgress { get; set; }
	}
}