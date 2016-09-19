using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace mpupdater
{
	public static class FileVersion
	{
		public static void WriteToFile(this Version instance, string path)
		{
			using (var versionFileStream = new StreamWriter(path, false))
			{
				versionFileStream.Write(instance);
			}
		}

		public static readonly Version Zero = new Version(0, 0);

		public static Version FromExecutable(string path)
		{
			FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
			return new Version((UInt16)info.FileMajorPart, (UInt16)info.FileMinorPart, (UInt16)info.FileBuildPart, (UInt16)info.FilePrivatePart);
		}

		public static Version FromFile(string path)
		{
			using (var versionFileStream = new StreamReader(path))
				return new Version(versionFileStream.ReadLine());
		}

		private const string versionRegexPattern = @"(\d+\.\d+(?:\.\d+)?(?:\.\d+)?)";

		public static Version FromWebResource(string url, string regexPrefix = "")
		{
			return FromWebResource(new Uri(url), regexPrefix);
		}

		public static Version FromWebResource(Uri url, string regexPrefix = "")
		{
			using (var client = new WebClient())
			{
				// Some hosts don't like weird user agents, so pretend we're IE11.
				client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");

				string pageData = client.DownloadString(url);
				Match versionInfo = Regex.Match(pageData, regexPrefix + versionRegexPattern);

				if (!versionInfo.Success)
					throw new FormatException("Couldn't parse version info at URL.");

				return new Version(versionInfo.Groups[1].Value);
			}
		}
	}
}
