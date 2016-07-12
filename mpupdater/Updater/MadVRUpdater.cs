
using System.IO;

namespace mpupdater
{
	public sealed class MadVRUpdater : ComFilterUpdater
	{
		private const string LOCAL_VERSION_FILE_NAME = "version.txt";
		private const string MADVR_VERSION_URL = "madVR/version.txt";
		private const string MADVR_ROOT_URL = "http://madshi.net/";

		#region Properties
#if WIN64
		protected override string filterDll => "madVR64.ax";
#else
		protected override string filterDll => "madVR.ax";
#endif
		protected override string filterPath => "./madVR";

		public override string Name => "madVR";
#if !DEBUG_NET
		protected override string UpdateRootUrl => MADVR_ROOT_URL;
		protected override string VersionUrl => MADVR_ROOT_URL + MADVR_VERSION_URL;
#endif
		protected override string UpdateRelativeUrl => "madVR.zip";
		#endregion

		protected override void PostInstallAction()
		{
			base.PostInstallAction();
			AvailableVersion.WriteToFile(Path.Combine(filterPath, LOCAL_VERSION_FILE_NAME));
		}

		protected override void GetInstalledVersion()
		{
			string path = Path.Combine(filterPath, LOCAL_VERSION_FILE_NAME);
			if (File.Exists(path))
				InstalledVersion = FileVersion.FromFile(path);
		}
	}
}
