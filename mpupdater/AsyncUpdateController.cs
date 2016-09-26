using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace mpupdater
{
	/// <summary>
	/// Provides simple control over multiple IUpdater instances, offloading network operations to separate threads.
	/// </summary>
	public class AsyncUpdateController
	{
		#region UpdaterCallbacks
		private static readonly EventHandler<UpdateCheckFinishedEventArgs> checkFinishedCallback
			= (sender, e) =>
			{
				string message = $"{(sender as IUpdater).Name} - ";

				if (e.AvailableVersion.CompareTo(e.InstalledVersion) > 0)
					message += $"{e.AvailableVersion} > {e.InstalledVersion} - Update found";
				else
					message += $"{e.InstalledVersion} is current - No update";

				ConsoleExt.InvokeAsync(() => Console.WriteLine(message));
			};

		private static readonly EventHandler startingInstallCallback
			= (sender, e) => ConsoleExt.InvokeAsync(() => Console.WriteLine($"{(sender as IUpdater).Name} - Installing"));
		private static readonly EventHandler updateFinishedCallback
			= (sender, e) => ConsoleExt.InvokeAsync(() => Console.WriteLine($"{(sender as IUpdater).Name} - Done"));

		private static readonly EventHandler comPreInstallCallback
			= (sender, e) => ConsoleExt.InvokeAsync(() => Console.WriteLine($"{(sender as IUpdater).Name} - Unregistering old version"));
		private static readonly EventHandler comPostInstallCallback
			= (sender, e) => ConsoleExt.InvokeAsync(() => Console.WriteLine($"{(sender as IUpdater).Name} - Registering new version"));

		public void AssignDefaultCallbacks()
		{
			foreach (var updater in updatesToPerform)
			{
				updater.UpdateCheckFinished += checkFinishedCallback;
				updater.StartingInstall += startingInstallCallback;
				updater.UpdateFinished += updateFinishedCallback;

				if (updater is ComFilterUpdater)
				{
					updater.PerformingPreInstallActions += comPreInstallCallback;
					updater.PerformingPostInstallActions += comPostInstallCallback;
				}
			}
		}
		#endregion

		public enum UpdateDownloadType
		{
			ToMemory,
			ToFile
		}

		public UpdateDownloadType DownloadType { get; set; }

		private readonly ConcurrentBag<IUpdater> updatesToInstall; // ie. updates that are available and didn't fault during the check.
		private readonly IEnumerable<IUpdater> updatesToPerform;

		public AsyncUpdateController(params IUpdater[] updateQueue)
		{
			DownloadType = UpdateDownloadType.ToFile;
			updatesToInstall = new ConcurrentBag<IUpdater>();
			updatesToPerform = updateQueue;
		}

		public async Task CheckUpdatesAsync()
		{
			var tasks = new List<Task>();

			foreach (var update in updatesToPerform)
			{
				tasks.Add(Task.Run(() =>
				{
					try
					{
						update.CheckUpdate();

						if (update.UpdateAvailable)
							updatesToInstall.Add(update);
					}
					catch (UpdaterException x)
					{
						UpdateFailed(update, x.Message);
					}
				}));
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);	
		}

		private async Task<Stream> DownloadUpdateAsync(IUpdater update)
		{
			ConsoleProgressBar progressBar = ConsoleProgressBar.Create($"{update.Name} - Downloading: ");
			var progressReportCallback = new Progress<double>((perc) => progressBar.Draw(perc));

			var downloadClient = new WebRequestDownloadClient(update.AbsoluteUpdateUrl);

			if (DownloadType == UpdateDownloadType.ToMemory)
			{
				var downloadTask = downloadClient.DownloadDataAsync(progressReportCallback);
				return new MemoryStream(await downloadTask.ConfigureAwait(false));
			}
			else if (DownloadType == UpdateDownloadType.ToFile)
			{
				var destination = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				var downloadTask = downloadClient.DownloadFileAsync(destination, progressReportCallback);

				await downloadTask.ConfigureAwait(false);
				File.SetAttributes(destination, FileAttributes.Temporary);

				return new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
			}
			
			return null;
		}

		public async Task DownloadAndInstallUpdatesAsync()
		{
			var tasks = new HashSet<Task<Stream>>();
			var updateTaskMapping = new Dictionary<Task, IUpdater>();

			// download concurrently
			IUpdater current;
			while(updatesToInstall.TryTake(out current))
			{
				var task = DownloadUpdateAsync(current);
				tasks.Add(task);
				updateTaskMapping[task] = current;
			}

			// install one at a time as downloads complete
			while (tasks.Count > 0)
			{
                Task<Stream> finishedTask = null;
				try
				{
					finishedTask = await Task.WhenAny(tasks).ConfigureAwait(false);
					var updateDataStream = await finishedTask.ConfigureAwait(false);

					using (updateDataStream)
							updateTaskMapping[finishedTask].Execute(updateDataStream);
				}
				catch (Exception x) when (x is System.Net.WebException || x is IOException || x is UpdaterException)
				{
					UpdateFailed(updateTaskMapping[finishedTask], x.Message);
				}
				finally
				{
					tasks.Remove(finishedTask);
				}
			}
		}

		private void UpdateFailed(IUpdater update, string message)
		{
			ConsoleExt.InvokeAsync(() => Console.Error.WriteLine($"{update.Name} - Failed: {message}"));
		}
	}
}
