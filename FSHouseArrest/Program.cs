using CommandLine;
using CommandLine.Text;
using MobileDevice;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace FSHouseArrest
{
	class Program
	{
		static void Main(string[] args)
		{
			var options = new Options();
			if (Parser.Default.ParseArguments(args, options))
			{
				var iPhone = new iPhone();
				iPhone.Connect += (sender, e) =>
				{
					iPhone.HouseArrestBundleIdentifier = options.HouseArrestBundleIdentifier;

					var root = new DirectoryInfo(options.SourceDirectory).Name;
					var files = Directory.EnumerateFiles(options.SourceDirectory, "*.*", SearchOption.AllDirectories).ToList();
					for (int i = 0; i < files.Count; i++)
					{
						var file = files[i];
						var remoteFolder = Path.Combine(options.DestinationDirectory,
							new FileInfo(file).DirectoryName.Substring(file.IndexOf(root) + root.Length + 1)).Replace(@"\", "/");
						CreateRemoteDirectory(iPhone, remoteFolder);
						CopyFile(iPhone, file, Path.Combine(remoteFolder, Path.GetFileName(file)).Replace(@"\", "/"),
								 ((double)i / (double)files.Count) * 100.0);
					}
				};

				while (true)
				{
					Thread.Sleep(50);
				}
			}
		}

		public static void CreateRemoteDirectory(iPhone iPhone, string directory)
		{
			//Log("Create Directory", directory);
			iPhone.CreateDirectory(directory, iPhone.ConnectMode.HouseArrest);
		}

		public static void CopyFile(iPhone iPhone, string source, string destination, double percentage)
		{
			Log("OK " + Math.Round(percentage, 2) + "%", destination);
			iPhone.CopyFile(source, destination, iPhone.ConnectMode.HouseArrest);
		}

		public static void Log(string header, string line)
		{
			WriteColor("[", ConsoleColor.Gray);
			WriteColor(header, ConsoleColor.Green);
			WriteColor("]", ConsoleColor.Gray);
			WriteLineColor(" " + line, ConsoleColor.White);
		}

		private static void WriteLineColor(string line, ConsoleColor color)
		{
			var pColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(line);
			Console.ForegroundColor = pColor;
		}

		private static void WriteColor(string line, ConsoleColor color)
		{
			var pColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(line);
			Console.ForegroundColor = pColor;
		}
	}

	class Options
	{
		[Option('s', "destination", Required = true,
		  HelpText = "Directory to copy.")]
		public string SourceDirectory { get; set; }

		[Option('d', "destination", Required = true,
		  HelpText = "Target directory on iDevice.")]
		public string DestinationDirectory { get; set; }

		[Option('b', "bundleidentifier", HelpText = "Bundle identifier if using house arrest service.")]
		public string HouseArrestBundleIdentifier { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}