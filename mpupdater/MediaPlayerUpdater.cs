using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace mpupdater
{
	public sealed class MediaPlayerUpdater : Updater
	{
		const string MediaPlayerPath = @"MPC-HC\";
		const string MediaPlayerExecutable = "mpc-hc.exe";

		private const string UpdateURL = "http://nightly.mpc-hc.org/";
		protected override string GetUpdateURL
		{
			get
			{
				return UpdateURL;
			}
		}

		protected override string GetVersionURL
		{
			get
			{
				return UpdateURL;
			}
		}

		protected override string GetVersionRegexPattern
		{
			get
			{
				return @"MPC-HC\.(\d+)\.(\d+)\.(\d+)\.(\d+)\.x86.7z";
			}
		}

		protected override void GetInstalledVersion()
		{
			try
			{
				IOExt.GetFileVersion(Path.Combine(MediaPlayerPath, MediaPlayerExecutable), out InstalledVersion.Major, out InstalledVersion.Minor, out InstalledVersion.Private, out InstalledVersion.Build);
				InstalledVersion.Installed = true;
			}
			catch (FileNotFoundException)
			{ }
		}

		public override void Update()
		{
			if (!CheckUpdate())
				return;

			string fileName = "MPC-HC." + CurrentVersion + ".x86.7z";

			Console.WriteLine("Downloading update...");
			DownloadUpdateWithProgress(fileName);
						
			Console.WriteLine("Extracting...");

			try
			{
				IOExt.ExtractSevenZip(fileName);
				IOExt.MoveDirWithOverwrite(Path.GetFileNameWithoutExtension(fileName), MediaPlayerPath);
			}
			finally
			{
				File.Delete(fileName);
			}
			
			Console.WriteLine("Done.");
		}
	}
}
