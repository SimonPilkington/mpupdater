using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace mpupdater.Tests
{
	[TestClass()]
	public class UpdaterTests
	{
		[TestMethod()]
		public void UpdaterTest_States()
		{
			var test = new DummyUpdater();

			Assert.IsTrue(test.State == Updater.UpdaterState.Pending, "Did not init correctly.");
			test.CheckUpdate();

			Assert.IsTrue(test.State == Updater.UpdaterState.Checked, "Did not move to checked state.");

			var dummyStream = Stream.Null;

			test.Execute(dummyStream);
			Assert.IsTrue(test.State == Updater.UpdaterState.Finished, "Did not move to finished state.");
		}

		[TestMethod()]
		public void CheckUpdateTest_VersionsNotNullAfterCheck()
		{
			var test = new DummyUpdater();

			test.CheckUpdate();

			Assert.IsNotNull(test.InstalledVersion);
			Assert.IsNotNull(test.AvailableVersion);
		}

		[TestMethod()]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ExecuteTest_ThrowOnExecuteBeforeCheck()
		{
			var test = new DummyUpdater();
			var dummyStream = Stream.Null;

			test.Execute(dummyStream);
		}

		[TestMethod()]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ExecuteTest_ThrowOnExecuteTwice()
		{
			var test = new DummyUpdater();
			var dummyStream = Stream.Null;

			try
			{
				test.CheckUpdate();
				test.Execute(dummyStream);
			}
			catch (InvalidOperationException x)
			{
				Assert.Fail("Unexpected exception: {0}", x);
			}
			
			test.Execute(dummyStream);
		}

		[TestMethod()]
		public void EventsTest()
		{
			var test = new DummyUpdater();

			bool updateCheckFinishedFired = false;
			test.UpdateCheckFinished += (s, e) => { updateCheckFinishedFired = true; };

			bool startingInstallFired = false;
			test.StartingInstall += (s, e) => { startingInstallFired = true; };

			bool performingPreInstallActionsFired = false;
			test.PerformingPreInstallActions += (s, e) => { performingPreInstallActionsFired = true; };

			bool performingPostInstallActionsFired = false;
			test.PerformingPostInstallActions += (s, e) => { performingPostInstallActionsFired = true; };

			bool updateFinishedFired = false;
			test.UpdateFinished += (s, e) => { updateFinishedFired = true; };

			test.CheckUpdate();

			var dummyStream = Stream.Null;
			test.Execute(dummyStream);

			Assert.IsTrue(updateCheckFinishedFired, "UpdateCheckFinished did not fire.");
			Assert.IsTrue(startingInstallFired, "StartingInstall did not fire.");
			Assert.IsTrue(performingPreInstallActionsFired, "PerformingPreInstallActions did not fire.");
			Assert.IsTrue(performingPostInstallActionsFired, "PerformingPostInstallActions did not fire.");
			Assert.IsTrue(updateFinishedFired, "UpdateFinished did not fire.");
		}
	}
}