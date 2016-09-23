using System;
using System.IO;

namespace mpupdater
{
	public sealed class SubFilterUpdater : ComFilterUpdater
	{
		#region Properties
		protected override string filterDll => "XySubFilter.dll";
		protected override string filterPath => "./XySubFilter";

		public override string Name => "xySubFilter";
#if !DEBUG_NET
		protected override string UpdateRootUrl => "https://github.com/Cyberbeing/xy-VSFilter/releases/download/";
		protected override string VersionUrl => "http://forum.doom9.org/showthread.php?t=168282"; 
#endif

		protected override string VersionSearchPrefix => @"XySubFilter beta version is:.+?";

#if WIN64
		protected override string UpdateRelativeUrl => $"{AvailableVersion}/XySubFilter_{AvailableVersion}_x64_BETA3.zip";
#else
		protected override string UpdateRelativeUrl => $"{AvailableVersion}/XySubFilter_{AvailableVersion}_x86_BETA3.zip";
#endif
		#endregion

		protected override void GetInstalledVersion()
		{
			string path = Path.Combine(filterPath, filterDll);
			if (File.Exists(path))
				InstalledVersion = NumberVersion.FromExecutable(path);
		}
	}
}
