using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace mpupdater
{
	public abstract class Updater
	{
		public struct Version
		{
			public int Major;
			public int Minor;
			public int Private;
			public int Build;

			public bool Installed
			{
				get;
				set;
			}

			public override string ToString()
			{
				if (!Installed)
					return "None";
				
				if (Private == -1)
					return String.Format("{0}.{1}.{2}", Major, Minor, Build);
				else
					return String.Format("{0}.{1}.{3}.{2}", Major, Minor, Private, Build);
			}
		}
	
		protected Version CurrentVersion;
		protected Version InstalledVersion;

		private const string UpdateURL = "http://localhost/";
        object consoleLock = new object();

        protected virtual string GetUpdateURL
		{
			get
			{
				return UpdateURL;
			}
		}

		protected virtual string GetVersionURL
		{
			get
			{
				return UpdateURL;
			}
		}

		protected abstract string GetVersionRegexPattern
		{
			 get;
		}

		protected abstract void GetInstalledVersion();

		protected void GetCurrentVersion()
		{
			Match versionInfo;
			using (var client = new WebClient())
			{
				string pageData = client.DownloadString(GetVersionURL);
				versionInfo = Regex.Match(pageData, GetVersionRegexPattern);
			}

			ExtractVersionFromMatch(versionInfo, ref CurrentVersion);
		}

		protected static void ExtractVersionFromMatch(Match versionInfo, ref Version output)
		{
			if (versionInfo == null)
				throw new ArgumentNullException("versionInfo");

			if (!versionInfo.Success)
				throw new UpdateCheckException("Couldn't get current version.");

			try
			{
				output.Major = int.Parse(versionInfo.Groups[1].Value);
				output.Minor = int.Parse(versionInfo.Groups[2].Value);
				output.Private = versionInfo.Groups[4].Value == "" ? -1 : int.Parse(versionInfo.Groups[4].Value);
				output.Build = int.Parse(versionInfo.Groups[3].Value);
				output.Installed = true;
			}
			catch (FormatException)
			{
				throw new UpdateCheckException("Somehow failed to parse " + versionInfo.Value + " for integers.");
			}
		}

		protected void DownloadUpdateWithProgress(string fileName)
		{
			using (var downloader = new WebClient())
			{
				var completeEvent = new ManualResetEventSlim();
				
				Exception error = null;

				int maxPercent = 0;
				downloader.DownloadProgressChanged += (sender, e) =>
				{
					if (e.ProgressPercentage > maxPercent)
						maxPercent = e.ProgressPercentage;
					else
						return;

					lock (consoleLock)
					{
						ConsoleExt.DrawProgressBar(e.ProgressPercentage, 25, '=');
					}
				};

				downloader.DownloadFileCompleted += (sender, e) =>
				{
                    lock (consoleLock)
                    {
                        Console.WriteLine();
                    }

					error = e.Error;
					completeEvent.Set();
				};

				downloader.DownloadFileAsync(new Uri(GetUpdateURL + fileName), Path.GetFileName(fileName));
				completeEvent.Wait();

				if (error != null)
					throw error;
			}
		}

		protected bool PrepareUpdate()
		{
			GetInstalledVersion();
			GetCurrentVersion();

			if (!InstalledVersion.Installed)
				return true;

			if (CurrentVersion.Major > InstalledVersion.Major ||
				(CurrentVersion.Major == InstalledVersion.Major && CurrentVersion.Minor > InstalledVersion.Minor) ||
				(CurrentVersion.Major == InstalledVersion.Major && CurrentVersion.Minor == InstalledVersion.Minor && CurrentVersion.Build > InstalledVersion.Build) ||
				(CurrentVersion.Major == InstalledVersion.Major && CurrentVersion.Minor == InstalledVersion.Minor && CurrentVersion.Build == InstalledVersion.Build && CurrentVersion.Private > InstalledVersion.Private))
				return true;

			return false;
		}

		protected bool CheckUpdate()
		{
			bool updateAvailable = PrepareUpdate();

			Console.WriteLine("Installed version: " + InstalledVersion);
			Console.WriteLine("Available version: " + CurrentVersion);

			if (!updateAvailable)
			{
				Console.WriteLine("No update.");
				return false;
			}

			Console.WriteLine("Update found.");
			return true;
		}

		public abstract void Update();
#if false
        public abstract void Remove(); 
#endif
    }
}
