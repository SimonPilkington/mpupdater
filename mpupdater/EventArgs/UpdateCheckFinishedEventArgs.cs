using System;

namespace mpupdater
{
	public class UpdateCheckFinishedEventArgs : EventArgs
	{
		public Version InstalledVersion { get; }
		public Version AvailableVersion { get; }

		public UpdateCheckFinishedEventArgs(Version installedVersion, Version availableVersion)
		{
			InstalledVersion = installedVersion;
			AvailableVersion = availableVersion;
		}
	}
}
