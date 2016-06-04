using System;
using System.Reflection;

namespace mpupdater
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.CursorVisible = false;

#if WIN64
			string architecture = "x64";
#else
			string architecture = "x86";
#endif
			Console.Title = $"Video nonsense updater";
			Console.WriteLine($"Video nonsense updater - version {Assembly.GetExecutingAssembly().GetName().Version} {architecture}");
			Console.WriteLine();

			IUpdater[] updaters = new IUpdater[] { new MediaPlayerUpdater(), new MadVRUpdater(), new SubFilterUpdater() };

			var testController = new AsyncUpdateController(updaters);

#if !DEBUG
			try
			{
#endif
				testController.AssignDefaultCallbacks();
				testController.CheckUpdatesAsync().Wait();
				testController.DownloadAndInstallUpdatesAsync().Wait();
#if !DEBUG
			}
			catch (AggregateException x)
			{
				Console.Error.WriteLine("An unhandled exception occurred. Details have been written to error.txt in the current directory.");
				using (var writer = new System.IO.StreamWriter("error.txt"))
				{
					foreach (var innx in x.InnerExceptions)
					{
						writer.WriteLine(x);
						writer.WriteLine(x.StackTrace);
					}
				}
			}
#endif

			Console.ReadKey();
		}
	}
}
