using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace mpupdater
{
	class Program
	{
#if WIN64
		const string architecture = "x64";
#else
		const string architecture = "x86";
#endif

		static void Main(string[] args)
		{
			Console.CursorVisible = false;

			Console.Title = $"Video nonsense updater";
			Console.WriteLine($"Video nonsense updater - version {Assembly.GetExecutingAssembly().GetName().Version} {architecture}");
			Console.WriteLine();

			bool doUpdate = true;
			if (args.Length > 0)
			{
				doUpdate = false;

				OptionSet commandLineOptions = null;
				commandLineOptions = new OptionSet()
				{
					{"h|?|help", "Show this help.", _ => commandLineOptions.WriteOptionDescriptions(Console.Out) },
					{"c|configure", "Configure the program.", _ =>
						{
							SetupConfig();

							doUpdate = ConsolePrompt.Create("Settings saved. Update now?");
							Console.WriteLine();
						}
					}
				};

				commandLineOptions.Parse(args);
			}

			if (doUpdate)
			{
				SingleThreadedExecutionMessageQueue messageQueue = SetupMessageQueue();

				Task update = UpdateAsync();
				update.ContinueWith(_ => messageQueue.TerminateMessageLoop());
				messageQueue.EnterMessageLoop();
			}

			Console.ReadKey();
		}

		static async Task UpdateAsync()
		{

#if !DEBUG
			try
			{
#endif
			var updates = new List<IUpdater>();
			if (Properties.Settings.Default.UpdateMpchc)
				updates.Add(new MediaPlayerUpdater());
			if (Properties.Settings.Default.UpdateMadvr)
				updates.Add(new MadVRUpdater());
			if (Properties.Settings.Default.UpdateXySubFIlter)
				updates.Add(new SubFilterUpdater());
			if (Properties.Settings.Default.UpdateFfmpeg)
				updates.Add(new FfmpegUpdater());

			var controller = new AsyncUpdateController(updates.ToArray());

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

		private static void SetupConfig()
		{
			Properties.Settings.Default.UpdateMpchc = ConsolePrompt.Create("Update MPC-HC?");
			Properties.Settings.Default.UpdateMadvr = ConsolePrompt.Create("Update MadVR?");
			Properties.Settings.Default.UpdateXySubFIlter = ConsolePrompt.Create("Update xySubFilter?");
			Properties.Settings.Default.UpdateFfmpeg = ConsolePrompt.Create("Update FFmpeg?");
			Properties.Settings.Default.Save();
		}

		static SingleThreadedExecutionMessageQueue SetupMessageQueue()
		{
			var messageQueue = new SingleThreadedExecutionMessageQueue();
			ConsoleExt.ConsoleMessageQueue = messageQueue;
			SynchronizationContext.SetSynchronizationContext(messageQueue.DefaultSynchronizationContext);
			return messageQueue;
		}
	}
}
