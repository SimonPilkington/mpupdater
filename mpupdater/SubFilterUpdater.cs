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

		public static void Unregister()
		{
			try
			{
				using (var xySvr = new RegSvr(Path.Combine(SubFilterPath, "XySubFilter.dll")))
				{
					xySvr.Unregister();
				}
			}
			catch (Exception e)
			{
				if (e is IOException)
					throw new UpdateCheckException("Failed to open xySubFilter dll.");

				if (e is ArgumentException)
					throw new UpdateCheckException("Something is wrong with the xySubFilter dll?");

				throw;
			}
		}

		public static void Register()
		{
			try
			{
				using (var xySvr = new RegSvr(Path.Combine(SubFilterPath, "XySubFilter.dll")))
				{
					xySvr.Register();
				}
			}
			catch (Exception e)
			{
				if (e is IOException)
					throw new UpdateCheckException("Failed to open xySubFilter dll.");

				if (e is ArgumentException)
					throw new UpdateCheckException("Something is wrong with the xySubFilter dll?");

				throw;
			}
		}

		public override void Update()
		{
			if (!CheckUpdate())
				return;

			if (InstalledVersion.Installed)
			{
				Console.WriteLine("Unregistering old version...");
				Unregister();
			}
#if WIN64
			string url = CurrentVersion + "/XySubFilter_" + CurrentVersion + "_x64_BETA2.zip";
#else
			string url = CurrentVersion + "/XySubFilter_" + CurrentVersion + "_x86_BETA2.zip";
#endif

			Console.WriteLine("Downloading update...");
			DownloadUpdateWithProgress(url);

			Console.WriteLine("Extracting...");

			string fileName = Path.GetFileName(url);
			const string tempDir = "SubFilter_temp";
			
			try
			{
				ZipFile.ExtractToDirectory(fileName, tempDir);

				try 
				{
					IOExt.MoveDirWithOverwrite(tempDir, SubFilterPath);
				}
				catch (UnauthorizedAccessException)
				{
					throw new UpdateCheckException("Could not overwrite old version. Is the player running?");
				}

				Console.WriteLine("Registering new version...");
				Register();				
			}
			catch (UpdaterException)
			{
				Directory.Delete(SubFilterPath, true);
				throw;
			}
			finally
			{
				if (File.Exists(fileName))
					File.Delete(fileName);

				if (Directory.Exists(tempDir))
					Directory.Delete(tempDir, true);
			}
			
			Console.WriteLine("Done.");
		}
	}
}
