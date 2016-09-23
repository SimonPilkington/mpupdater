using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace mpupdater
{
	public class NumberVersion : IVersion
	{
		private const string versionRegexPattern = @"(\d+\.\d+(?:\.\d+)?(?:\.\d+)?)";
		public static readonly NumberVersion NotInstalled = new NumberVersion();

		public Version internalVersion { get; private set; }

		public bool Installed => internalVersion != null;
						
		public override int GetHashCode()
		{
			return internalVersion.GetHashCode();
		}

		#region Comparers
		public static bool operator ==(NumberVersion first, NumberVersion second)
		{
			return first?.internalVersion == second?.internalVersion;
		}

		public static bool operator !=(NumberVersion first, NumberVersion second)
		{
			return !(first == second);
		}

		public static bool operator >(NumberVersion first, NumberVersion second)
		{
			return first?.internalVersion > second?.internalVersion;
		}

		public static bool operator <(NumberVersion first, NumberVersion second)
		{
			return first.internalVersion < second.internalVersion;
		}

		public static bool operator >=(NumberVersion first, NumberVersion second)
		{
			return !(first < second);
		}

		public static bool operator <=(NumberVersion first, NumberVersion second)
		{
			return !(first > second);
		}

		public int CompareTo(IVersion obj)
		{
			var other = obj as NumberVersion;

			if (other == null)
				return 1;

			return internalVersion?.CompareTo(other.internalVersion) ?? -1;
		}

		public override bool Equals(object obj)
		{
			var other = obj as NumberVersion;
			if (other == null)
				return false;

			return other.internalVersion.Equals(internalVersion);
		}
		#endregion

		public void WriteToFile(string path)
		{
			using (var versionFileStream = new StreamWriter(path, false))
			{
				versionFileStream.Write(this.internalVersion);
			}
		}

		public static NumberVersion Parse(string value)
		{
			return new NumberVersion { internalVersion = new Version(value) };
		}

		public static NumberVersion FromFile(string path)
		{
			using (var versionFileStream = new StreamReader(path))
				return Parse(versionFileStream.ReadLine());
		}

		public static NumberVersion FromExecutable(string path)
		{
			FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
			return new NumberVersion
			{
				internalVersion = new Version((UInt16)info.FileMajorPart, (UInt16)info.FileMinorPart, (UInt16)info.FileBuildPart, (UInt16)info.FilePrivatePart)
			};
		}

		public static NumberVersion FromWebResource(string url, string regexPrefix = "")
		{
			return FromWebResource(new Uri(url), regexPrefix);
		}

		public static NumberVersion FromWebResource(Uri url, string regexPrefix = "")
		{
			using (var client = new WebClient())
			{
				// Some hosts don't like weird user agents, so pretend we're IE11.
				client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");

				string pageData = client.DownloadString(url);
				Match versionInfo = Regex.Match(pageData, regexPrefix + versionRegexPattern);

				if (!versionInfo.Success)
					throw new FormatException("Couldn't parse version info at URL.");

				return Parse(versionInfo.Groups[1].Value);
			}
		}

		public override string ToString()
		{
			return internalVersion?.ToString() ?? "Not installed";
		}
	}
}
