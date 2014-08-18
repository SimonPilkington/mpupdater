using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpupdater
{
	public sealed class SubFilterUpdater : Updater
	{
		const string SubFilterDll = "XySubFilter.dll";
		const string SubFilterPath = @"XySubFilter\";

		protected override string GetUpdateURL
		{
			get
			{
				return @"https://github.com/Cyberbeing/xy-VSFilter/releases/download/";
			}
		}

		protected override string GetVersionRegexPattern
		{
			get
			{
				return @"XySubFilter beta version is:.+?(\d+)\.(\d+)\.(\d+)\.(\d+)";
			}
		}

		protected override string GetVersionURL
		{
			get
			{
				return @"https://code.google.com/p/xy-vsfilter/wiki/Downloads";
			}
		}

		protected override void GetInstalledVersion()
		{
			try
			{
				IOExt.GetFileVersion(Path.Combine(SubFilterPath, SubFilterDll), out InstalledVersion.Major, out InstalledVersion.Minor, out InstalledVersion.Private, out InstalledVersion.Build);
				InstalledVersion.Installed = true;
			}
			catch (FileNotFoundException)
			{ }
		}

		public static void RunUninstallScript()
		{
			int exitCode = IOExt.RunProcess(Path.Combine(SubFilterPath, "Uninstall_XySubFilter.bat"));

			if (exitCode != 0)
				throw new UpdateCheckException("Something went wrong with madVR uninstallation. (Exit code: " + exitCode + ")");
		}

		public static void RunInstallScript()
		{
			int exitCode = IOExt.RunProcess(Path.Combine(SubFilterPath, "Install_XySubFilter.bat"));

			if (exitCode != 0)
				throw new UpdateCheckException("Something went wrong with madVR installation. (Exit code: " + exitCode + ")");
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

			string url = CurrentVersion + "/XySubFilter_" + CurrentVersion + "_x86_BETA2.zip";

			Console.WriteLine("Downloading update...");
			DownloadUpdateWithProgress(url);

			Console.WriteLine("Extracting...");

			string fileName = Path.GetFileName(url);
			const string tempDir = "SubFilter_temp";
			
			try
			{
				ZipFile.ExtractToDirectory(fileName, tempDir);
				IOExt.MoveDirWithOverwrite(tempDir, SubFilterPath);

				Console.WriteLine("Installing new version...");
				RunInstallScript();				
			}
			catch (UpdaterException)
			{
				Directory.Delete(SubFilterPath, true);
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
