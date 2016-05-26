using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace mpupdater
{
	public sealed class MadVRUpdater : Updater
	{
		private const string madVrPath = "./madVR/";
		private const string versionFilename = "version.bin";
		private const string madVRVersionURL = "madVR/version.txt";
		private const string siteUrl = "http://madshi.net/";

#if WIN64
		const string madVrFilename = "madVR64.ax";
#else
		const string madVrFilename = "madVR.ax";
#endif

#if !DEBUG_NET
		protected override string UpdateUrl => siteUrl;
		protected override string VersionUrl => siteUrl + madVRVersionURL; 
#endif

		protected override string VersionRegexPattern => @"(\d+)\.(\d+)\.(\d+)\.(\d+)";

		protected override void GetInstalledVersion()
		{
			try
			{
				InstalledVersion = FileVersion.FromFileBin(Path.Combine(madVrPath, versionFilename));
			}
			catch (FileNotFoundException)
			{ }
		}

		private static void Unregister()
		{
#if DEBUG_NET
			return;
#endif
			try
			{
				RegSvr.UnregisterServer(Path.Combine(madVrPath, madVrFilename));
			}
			catch (Exception e)
			{
				if (e is IOException)
					throw new UpdateCheckException("Failed to open madVR dll.");

				if (e is ArgumentException)
					throw new UpdateCheckException("Something is wrong with the madVR dll?");

				throw;
			}
		}

		private static void Register()
		{
#if DEBUG_NET
			return;
#endif
			try
			{
				RegSvr.RegisterServer(Path.Combine(madVrPath, madVrFilename));
			}
			catch (Exception e)
			{
				if (e is IOException)
					throw new UpdateCheckException("Failed to open madVR dll.");

				if (e is ArgumentException)
					throw new UpdateCheckException("Something is wrong with the madVR dll?");

				throw;
			}
		}

		public void UpdateInstalledVersion()
		{
			AvailableVersion.ToFileBin(Path.Combine(madVrPath, versionFilename));
		}

		public override void Execute()
		{
			if (!CheckUpdate())
				return;

			if (InstalledVersion != FileVersion.Zero)
			{
				Console.WriteLine("Unregistering old version...");
				Unregister();
			}

			const string fileName = "madVR.zip";

			Console.WriteLine("Downloading update...");
			Stream fileStream = DownloadToMemoryWithProgress(fileName);

			Console.WriteLine("Extracting...");

			string tempDir = Path.Combine(Path.GetTempPath(), "madVR_temp");

			try
			{
				using (fileStream)
				{
					var extractor = new ZipArchive(fileStream);
					if (Directory.Exists(tempDir))
						Directory.Delete(tempDir, true);

					extractor.ExtractToDirectory(tempDir);

					try
					{
						IOExt.MoveDirWithOverwrite(tempDir, madVrPath);
					}
					catch (UnauthorizedAccessException)
					{
						throw new UpdateCheckException("Could not overwrite old version. Is the player running?");
					}
				}

				Console.WriteLine("Registering new version...");
				Register();
				UpdateInstalledVersion();
			}
			finally
			{
				if (Directory.Exists(tempDir))
					Directory.Delete(tempDir, true);
			}

			Console.WriteLine("Done.");
		}
	}
}
