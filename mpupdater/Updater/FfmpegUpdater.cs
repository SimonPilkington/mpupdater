using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace mpupdater
{
	public class FfmpegUpdater : Updater
	{
#if WIN64
		private const string FFMPEG_PATH = "./ffmpeg-latest-win64-shared";
#else
		private const string FFMPEG_PATH = "./ffmpeg-latest-win32-shared";
#endif

#region Properties
		public override string Name => "FFmpeg";

#if WIN64
#if !DEBUG_NET
		protected override string UpdateRootUrl => "https://ffmpeg.zeranoe.com/builds/win64/shared/";	
#endif //DEBUG_NET
		protected override string UpdateRelativeUrl => "ffmpeg-latest-win64-shared.zip";
#else
#if !DEBUG_NET
		protected override string UpdateRootUrl => "https://ffmpeg.zeranoe.com/builds/win32/shared/";
#endif //DEBUG_NET
		protected override string UpdateRelativeUrl => "ffmpeg-latest-win32-shared.zip";
#endif //WIN64

#if !DEBUG_NET
		protected override string VersionUrl => UpdateRootUrl + @"?C=M&O=D";
#endif
#endregion

		protected override void GetInstalledVersion()
		{
			const string versionRegexString = @"Build: ffmpeg-(\d{4})(\d{2})(\d{2})";
			string readmePath = Path.Combine(FFMPEG_PATH, "README.txt");

			if (!File.Exists(readmePath))
				return;

			var versionRegex = new Regex(versionRegexString, RegexOptions.IgnoreCase);
			Match versionLineMatch = null;
			using (var stream = new StreamReader(readmePath))
			{
				do
				{
					string line = stream.ReadLine();
					versionLineMatch = versionRegex.Match(line);
				} while (!versionLineMatch.Success && !stream.EndOfStream);
			}

			// Corrupt installation? Format change? Either way, proceed with update and hope that fixes it??
			if (!versionLineMatch.Success)
				return;

			string year = versionLineMatch.Groups[1].Value;
			string month = versionLineMatch.Groups[2].Value;
			string day = versionLineMatch.Groups[3].Value;

			InstalledVersion = TimestampVersion.Parse($"{year}-{month}-{day}");
		}

		protected override void GetAvailableVersion()
		{
			const string webVersionRegex = @"\d{4}-\w{3}-\d{2}";

			try
			{
				AvailableVersion = TimestampVersion.FromWebResource(VersionUrl, webVersionRegex);
			}
			catch (System.Net.WebException x)
			{
				throw new UpdaterException(x.Message, x);
			}
			catch (FormatException x)
			{
				throw new UpdaterException("Failed to parse available version. The resource may have changed or moved.", x);
			}
		}

		protected override void Install(Stream updateDataStream)
		{
			string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()); ;

			try
			{
				using (var extractor = new ZipArchive(updateDataStream))
					extractor.ExtractToDirectory(tempDir);

				IOExt.MoveDirWithOverwrite(Path.Combine(tempDir, FFMPEG_PATH), FFMPEG_PATH);
			}
			catch (InvalidDataException x)
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
