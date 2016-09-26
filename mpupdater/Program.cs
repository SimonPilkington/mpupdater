using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace mpupdater
{
	class Program
	{
		static async Task MainAsyncPart()
		{

#if !DEBUG
			try
			{
#endif
				IUpdater[] updaters = new IUpdater[] { new MediaPlayerUpdater(), new MadVRUpdater(), new SubFilterUpdater(), new FfmpegUpdater() };

				var controller = new AsyncUpdateController(updaters);

				controller.AssignDefaultCallbacks();
				await controller.CheckUpdatesAsync();
				await controller.DownloadAndInstallUpdatesAsync();
#if !DEBUG
			}

			catch (Exception x)
			{
				Console.Error.WriteLine("An unhandled exception occurred. Details have been written to error.txt in the current directory.");
				using (var writer = new System.IO.StreamWriter("error.txt"))
					writer.Write(x);
			}
#endif
		}

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

			var messageQueue = new SingleThreadedExecutionMessageQueue();
			ConsoleExt.ConsoleMessageQueue = messageQueue;
			SynchronizationContext.SetSynchronizationContext(messageQueue.DefaultSynchronizationContext);

			var asyncMainTask = MainAsyncPart();
			asyncMainTask.ContinueWith(_ => messageQueue.TerminateMessageLoop());

			messageQueue.EnterMessageLoop();

			Console.ReadKey();
		}
	}
}
