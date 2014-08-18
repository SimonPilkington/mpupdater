using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.IO.Compression;
using System.Diagnostics;
using System.ComponentModel;

namespace mpupdater
{
	public sealed class MadVRUpdater : Updater
	{
		const string MadVRPath = @"madVR\";
		const string MadVRVersionURL = @"madVR/version.txt";

		private const string UpdateURL = "http://madshi.net/";
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
				return UpdateURL + @"madVR/version.txt";
			}
		}

		protected override string GetVersionRegexPattern
		{
			get
			{
				return @"(\d+)\.(\d+)\.(\d+)\.(\d+)";
			}
		}

		protected override void GetInstalledVersion()
		{
			string versionString = null;
			try
			{
				using (var versionFile = new StreamReader(Path.Combine(MadVRPath, "installedVersion.txt")))
					versionString = versionFile.ReadToEnd();
			}
			catch (IOException e)
			{
				if (e is FileNotFoundException || e is DirectoryNotFoundException)
				{
					return;
				}
				else
					throw;
			}

			var versionInfo = Regex.Match(versionString, GetVersionRegexPattern);

			try
			{
				ExtractVersionFromMatch(versionInfo, ref InstalledVersion);
			}
			catch (UpdateCheckException)
			{
				InstalledVersion.Installed = false;
			}
		}

		public static void RunUninstallScript()
		{
			int exitCode = IOExt.RunProcess(MadVRPath + "uninstall.bat");

			if (exitCode != 0)
				throw new UpdateCheckException("Something went wrong with madVR uninstallation. (Exit code: " + exitCode + ")");
		}

		public static void RunInstallScript()
		{
			int exitCode = IOExt.RunProcess(MadVRPath + "install.bat");

			if (exitCode != 0)
				throw new UpdateCheckException("Something went wrong with madVR installation. (Exit code: " + exitCode + ")");
		}

		public void UpdateInstalledVersion()
		{
			using (var versionFile = new StreamWriter(Path.Combine(MadVRPath, "installedVersion.txt"), false))
				versionFile.Write(CurrentVersion);
		}

		public override void Update()
		{
			if (!CheckUpdate())
				return;

			if (InstalledVersion.Installed)
			{
				Console.WriteLine("Uninstalling old version...");
				RunUninstallScript();
			}

			const string fileName = "madVR.zip";

			Console.WriteLine("Downloading update...");
			DownloadUpdateWithProgress(fileName);

			Console.WriteLine("Extracting...");

			const string tempDir = "madVR_temp";

			try
			{
				ZipFile.ExtractToDirectory(fileName, tempDir);
				IOExt.MoveDirWithOverwrite(tempDir, MadVRPath);

				Console.WriteLine("Installing new version...");
				RunInstallScript();
				UpdateInstalledVersion();
			}
			catch (UpdaterException)
			{
				Directory.Delete(MadVRPath, true);
				throw;
			}
			finally
			{
				File.Delete(fileName);
			}

			Console.WriteLine("Done.");
		}
	}
}
