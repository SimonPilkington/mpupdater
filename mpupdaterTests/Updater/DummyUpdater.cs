using System.IO;

namespace mpupdater.Tests
{
	internal class DummyUpdater : Updater
	{
		public override string Name => "Dummy updater";

		protected override string UpdateRelativeUrl => string.Empty;

		protected override string VersionSearchPrefix => string.Empty;

		protected override bool PerformPreInstall => true;
		protected override bool PerformPostInstall => true;

		protected override void GetInstalledVersion()
		{
		}

		protected override void GetAvailableVersion()
		{
		}

		protected override void Install(Stream updateDataStream)
		{
		}
	}
}
