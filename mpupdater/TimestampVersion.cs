using System;
using System.Net;
using System.Text.RegularExpressions;

namespace mpupdater
{
	public class TimestampVersion : IVersion
	{
		public DateTime? internalVersion { get; private set; }

		public bool Installed => internalVersion != null;

		public override int GetHashCode()
		{
			return internalVersion.GetHashCode();
		}

		#region Comparers
		public static bool operator ==(TimestampVersion first, TimestampVersion second)
		{
			return first?.internalVersion == second?.internalVersion;
		}

		public static bool operator !=(TimestampVersion first, TimestampVersion second)
		{
			return !(first == second);
		}

		public static bool operator >(TimestampVersion first, TimestampVersion second)
		{
			return first?.internalVersion > second?.internalVersion;
		}

		public static bool operator <(TimestampVersion first, TimestampVersion second)
		{
			return first?.internalVersion < second?.internalVersion;
		}

		public static bool operator >=(TimestampVersion first, TimestampVersion second)
		{
			return !(first < second);
		}

		public static bool operator <=(TimestampVersion first, TimestampVersion second)
		{
			return !(first > second);
		}

		public int CompareTo(IVersion obj)
		{
			var other = obj as TimestampVersion;

			if (other == null)
				return 1;

			return internalVersion?.CompareTo(other.internalVersion) ?? -1;
		}

		public int CompareTo(TimestampVersion other)
		{
			if (other == null)
				return 1;

			return CompareTo((IVersion)other);
		}

		public override bool Equals(object obj)
		{
			var other = obj as TimestampVersion;
			if (other == null)
				return false;

			return other.internalVersion.Equals(internalVersion);
		}
		#endregion

		public static TimestampVersion Parse(string value)
		{
			return new TimestampVersion { internalVersion = DateTime.Parse(value) };
		}

		public static TimestampVersion FromWebResource(string url, string regex = "")
		{
			return FromWebResource(new Uri(url), regex);
		}

		public static TimestampVersion FromWebResource(Uri url, string regex = "")
		{
			using (var client = SpoofedWebClient.Create())
			{
				// Some hosts don't like weird user agents, so pretend we're IE11.
				client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");

				string pageData = client.DownloadString(url);
				Match versionInfo = Regex.Match(pageData, regex);

				if (!versionInfo.Success)
					throw new FormatException("Couldn't parse version info at URL.");

				return Parse(versionInfo.Groups[0].Value);
			}
		}

		public override string ToString()
		{
			return internalVersion?.ToString("yyyy-MM-dd") ?? "Not installed";
		}
	}
}
