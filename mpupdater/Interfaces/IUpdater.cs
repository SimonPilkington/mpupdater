using System;

namespace mpupdater
{
	public interface IUpdater
	{
		bool UpdateAvailable { get; }
		string AbsoluteUpdateUrl { get; }
		string Name { get; }
		void CheckUpdate();
		void Execute(System.IO.Stream updateDataStream);
		
		event EventHandler<UpdateCheckFinishedEventArgs> UpdateCheckFinished;
		event EventHandler PerformingPreInstallActions;
		event EventHandler StartingInstall;
		event EventHandler PerformingPostInstallActions;
		event EventHandler UpdateFinished;
	}
}
