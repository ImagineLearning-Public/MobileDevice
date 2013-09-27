using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MobileDevice;

namespace HouseArrestSync
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Queue<iPhoneSync> _devicesToSync;

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
			_devicesToSync = new Queue<iPhoneSync>();
			foreach (var iPhoneSync in Devices.Items.Cast<iPhoneSync>())
			{
				_devicesToSync.Enqueue(iPhoneSync);
			}

			var catalyst = _devicesToSync.Peek();
			catalyst.SyncCompleted += (s, args) => BeginSync();
			BeginSync();
		}

		private void BeginSync()
		{
			var syncWorker = new BackgroundWorker();
			syncWorker.DoWork += (s, args) =>
			{
				Dispatcher.BeginInvoke((Action) (() =>
				{
					var iPhoneSyncClosure = _devicesToSync.Dequeue();
					var iPhoneClosure = iPhoneSyncClosure.iPhone;
					iPhoneClosure.ConnectViaHouseArrest((string)Settings.Default["BundleIdentifier"]);

					var root = new DirectoryInfo(SourceDirectory.Text).Name;
					var files = Directory.EnumerateFiles(SourceDirectory.Text, "*.*", SearchOption.AllDirectories).ToList();
					for (int i = 0; i < files.Count; i++)
					{
						var file = files[i];
						var remoteFolder = Path.Combine(TargetDirectory.Text,
							new FileInfo(file).DirectoryName.Substring(file.IndexOf(root) + root.Length + 1)).Replace(@"\", "/");
						CreateRemoteDirectory(iPhoneClosure, remoteFolder);
						CopyFile(iPhoneSyncClosure, file, Path.Combine(remoteFolder, Path.GetFileName(file)).Replace(@"\", "/"),
							((double)i / (double)files.Count) * 100.0);
					}
				}));
			};
			syncWorker.RunWorkerAsync();
		}

		private void CreateRemoteDirectory(iPhone iPhone, string directory)
		{
			if (!iPhone.CreateDirectory(directory))
			{
				MessageBox.Show(string.Format("Create directory failed: {0}", directory));
			}
		}

		private void CopyFile(iPhoneSync iPhoneSync, string source, string destination, double percentage)
		{
			try
			{
				iPhoneSync.Progress = (int) percentage;
				iPhoneSync.iPhone.CopyFile(source, destination);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
	}

	public class iPhoneSync
	{
		public iPhone iPhone { get; set; }
		public int Progress { get; set; }
		public event EventHandler SyncCompleted;
	}
}
