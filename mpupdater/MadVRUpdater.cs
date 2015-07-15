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

#if WIN64
		const string madVrFilename = "madVR64.ax";
#else
		const string madVrFilename = "madVR.ax";
#endif

		public static void Unregister()
		{
			string oldDir = Directory.GetCurrentDirectory();
			
			try
			{
				Directory.SetCurrentDirectory(MadVRPath);
				using (var madVrSvr64 = new RegSvr(madVrFilename))
				{
					madVrSvr64.Unregister();
				}
			}
			catch (Exception e)
			{
				if (e is IOException)
					throw new UpdateCheckException("Failed to open madVR dll.");

				if (e is ArgumentException)
					throw new UpdateCheckException("Something is wrong with the madVR dll?");

				throw;
			}
			finally
			{
				Directory.SetCurrentDirectory(oldDir);
			}

		}

		public static void Register()
		{
			string oldDir = Directory.GetCurrentDirectory();
			try
			{
				Directory.SetCurrentDirectory(MadVRPath);

				using (var madVrSvr64 = new RegSvr(madVrFilename))
				{
					madVrSvr64.Register();
				}
			}
			catch (Exception e)
			{
				if (e is IOException)
					throw new UpdateCheckException("Failed to open madVR dll.");

				if (e is ArgumentException)
					throw new UpdateCheckException("Something is wrong with the madVR dll?");

				throw;
			}
			finally
			{
				Directory.SetCurrentDirectory(oldDir);
			}
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
				Console.WriteLine("Unregistering old version...");
				Unregister();
			}

			const string fileName = "madVR.zip";

			Console.WriteLine("Downloading update...");
			DownloadUpdateWithProgress(fileName);

			Console.WriteLine("Extracting...");

			const string tempDir = "madVR_temp";

			try
			{
				ZipFile.ExtractToDirectory(fileName, tempDir);

				try
				{
					IOExt.MoveDirWithOverwrite(tempDir, MadVRPath);
				}
				catch (UnauthorizedAccessException)
				{
					throw new UpdateCheckException("Could not overwrite old version. Is the player running?");
				}
				
				Console.WriteLine("Registering new version...");
				Register();
				UpdateInstalledVersion();
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
