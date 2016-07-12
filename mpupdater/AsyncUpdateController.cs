using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

				if (e.AvailableVersion > e.InstalledVersion)
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
			foreach (var updater in updateState.Keys)
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

		private readonly ConcurrentDictionary<IUpdater, bool> updateState; // true if update can continue, false if faulted during the check

		public AsyncUpdateController(params IUpdater[] updateQueue)
		{
			DownloadType = UpdateDownloadType.ToFile;
			updateState = new ConcurrentDictionary<IUpdater, bool>(updateQueue.ToDictionary(x => x, x => true));
		}

		public async Task CheckUpdatesAsync()
		{
			var tasks = new HashSet<Task>();

			foreach (var update in updateState.Keys)
			{
				tasks.Add(Task.Run(() =>
				{
					try
					{
						update.CheckUpdate();
					}
					catch (UpdaterException x)
					{
						UpdateFailed(update, x.Message);
						updateState[update] = false;
					}
				}));
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);	
		}

		private async Task<Stream> DownloadUpdateAsync(IUpdater update)
		{
			ConsoleProgressBar progressBar = ConsoleProgressBar.Create($"{update.Name} - Downloading: ");
			Progress<double> progressReportCallback = new Progress<double>((perc) => progressBar.Draw(perc));

			var downloadClient = new WebRequestDownloadClient(update.AbsoluteUpdateUrl);

			if (DownloadType == UpdateDownloadType.ToMemory)
			{
				var downloadTask = downloadClient.DownloadDataAsync(progressReportCallback);
				return new MemoryStream(await downloadTask.ConfigureAwait(false)); 
			}
			else //if (DownloadType == UpdateDownloadType.ToFile)
			{
				var destination = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				var downloadTask = downloadClient.DownloadFileAsync(destination, progressReportCallback);
				
				await downloadTask.ConfigureAwait(false);
				File.SetAttributes(destination, FileAttributes.Temporary);

				return new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
			}
		}

		public async Task DownloadAndInstallUpdatesAsync()
		{
			// filter out only those where an update is available, and where the update check did not fault
			var updates = from entry in updateState where
						  entry.Value == true && // update check did not fault
						  entry.Key.UpdateAvailable == true
						  select entry.Key;

			var tasks = new HashSet<Task<Stream>>();
			var updateTaskMapping = new Dictionary<Task, IUpdater>();

			// download concurrently
			foreach (var update in updates)
			{
				var task = DownloadUpdateAsync(update);
				tasks.Add(task);
				updateTaskMapping[task] = update;
			}

			// install one at a time as downloads complete
			while (tasks.Count > 0)
			{
				try
				{
					var finishedTask = await Task.WhenAny(tasks).ConfigureAwait(false);
					var updateDataStream = await finishedTask.ConfigureAwait(false);

					try
					{
						using (updateDataStream)
							updateTaskMapping[finishedTask].Execute(updateDataStream);
					}
					catch (UpdaterException x)
					{
						UpdateFailed(updateTaskMapping[finishedTask], x.Message);
					}
					finally
					{
						tasks.Remove(finishedTask);
					}
				}
				catch (Exception x) when (x is System.Net.WebException || x is IOException)
				{
					var faultedTask = tasks.Where(t => t.Status == TaskStatus.Faulted).First();
					UpdateFailed(updateTaskMapping[faultedTask], x.Message);
					tasks.Remove(faultedTask);
				}
			}
		}

		private void UpdateFailed(IUpdater update, string message)
		{
			ConsoleExt.InvokeAsync(() => Console.Error.WriteLine($"{update.Name} - Failed: {message}"));
		}
	}
}
