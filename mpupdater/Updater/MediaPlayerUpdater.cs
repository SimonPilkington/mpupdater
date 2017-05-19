using System.IO;

namespace mpupdater
{
	public sealed class MediaPlayerUpdater : Updater
	{
#if WIN64
		private const string MEDIA_PLAYER_PATH = "./MPC-HC64";
		private const string MEDIA_PLAYER_EXECUTABLE = "mpc-hc64.exe";
#else
		private const string MEDIA_PLAYER_PATH = "./MPC-HC";
		private const string MEDIA_PLAYER_EXECUTABLE = "mpc-hc.exe";
#endif

		#region Properties

		public override string Name => "MPC-HC";
#if !DEBUG_NET
		protected override string UpdateRootUrl => "https://nightly.mpc-hc.org/";
		protected override string VersionUrl => "https://nightly.mpc-hc.org/";
#endif
		
#if WIN64
		protected override string UpdateRelativeUrl => $"MPC-HC.{AvailableVersion}.x64.7z";
#else
		protected override string UpdateRelativeUrl => $"MPC-HC.{AvailableVersion}.x86.7z";
#endif

		protected override string VersionSearchPrefix => @"MPC-HC\.";
		#endregion

		protected override void GetInstalledVersion()
		{
			string path = Path.Combine(MEDIA_PLAYER_PATH, MEDIA_PLAYER_EXECUTABLE);
			if (File.Exists(path))
				InstalledVersion = NumberVersion.FromExecutable(path);
		}

		protected override void Install(Stream updateStream)
		{
			string tempDir = null;

			try
			{
				using (var extractor = new SevenZip.SevenZipExtractor(updateStream))
				{
					extractor.ExtractArchive(Path.GetTempPath());
					tempDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(UpdateRelativeUrl));
				}
				IOExt.MoveDirWithOverwrite(tempDir, MEDIA_PLAYER_PATH);
			}
			catch (SevenZip.SevenZipException x)
			{
				throw new UpdaterException(x.Message, x);
			}
			finally
			{
				if (tempDir != null && Directory.Exists(tempDir))
					Directory.Delete(tempDir, true);
			}
		}
	}
}
