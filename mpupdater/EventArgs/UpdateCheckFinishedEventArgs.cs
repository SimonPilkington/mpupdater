using System;

namespace mpupdater
{
	public class UpdateCheckFinishedEventArgs : EventArgs
	{
		public IVersion InstalledVersion { get; }
		public IVersion AvailableVersion { get; }

		public UpdateCheckFinishedEventArgs(IVersion installedVersion, IVersion availableVersion)
		{
			InstalledVersion = installedVersion;
			AvailableVersion = availableVersion;
		}
	}
}
