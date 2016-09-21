using System;
using System.IO;
using System.Net;

namespace mpupdater
{
	public abstract class Updater : IUpdater
	{
		#region Enums
		public enum UpdaterState
		{
			Pending,
			Checked,
			Finished
		}

		#endregion
		#region Properties
		public UpdaterState State
		{
			get;
			private set;
		}
		
		private Version _availableVersion;
		public Version AvailableVersion
		{
			get
			{
				if (State > UpdaterState.Pending)
					return _availableVersion;

				throw new InvalidOperationException("Value unavailable. CheckUpdate has not been called.");
			}
			protected set { _availableVersion = value; }
		}

		private Version _installedVersion;
		public Version InstalledVersion
		{
			get
			{
				if (State > UpdaterState.Pending)
					return _installedVersion;

				throw new InvalidOperationException("Value unavailable. CheckUpdate has not been called.");
			}
			protected set { _installedVersion = value; }
		}
		
		public bool UpdateAvailable
		{
			get
			{
				if (State > UpdaterState.Pending)
					return AvailableVersion > (InstalledVersion);

				throw new InvalidOperationException("Update check has not been performed. Call CheckUpdate first.");
			}
		}

		public abstract string Name { get; }
		protected virtual string UpdateRootUrl => "http://localhost/";
		protected abstract string UpdateRelativeUrl { get; }
		protected virtual string VersionUrl => "http://localhost/";
		public string AbsoluteUpdateUrl => Path.Combine(UpdateRootUrl, UpdateRelativeUrl);

		/// <summary>
		/// Prefix to be used in searching the resource for the available version. Uses regex syntax.
		/// </summary>
		protected virtual string VersionSearchPrefix => string.Empty;
		#endregion
		
		public Updater()
		{
			_installedVersion = FileVersion.Zero;
			_availableVersion = FileVersion.Zero;

			State = UpdaterState.Pending;
		}

		protected abstract void GetInstalledVersion();

		protected virtual void GetAvailableVersion()
		{
			try
			{
				AvailableVersion = FileVersion.FromWebResource(VersionUrl, VersionSearchPrefix);
			}
			catch (WebException x)
			{
				throw new UpdaterException(x.Message, x);
			}
			catch (FormatException x)
			{
				throw new UpdaterException("Failed to parse available version. The resource may have changed or moved.", x);
			}
		}
		
		protected virtual bool PerformPreInstall => false;
		/// <summary>
		/// Perfmorm pre-install actions. (ex. Unregistering existing version of a COM server.) (Optional.)
		/// </summary>
		protected virtual void PreInstallAction()
		{ }

		/// <summary>
		/// Install the update.
		/// </summary>
		/// <param name="updateDataStream">The stream containing update data.</param>
		protected abstract void Install(Stream updateDataStream);

		protected virtual bool PerformPostInstall => false;
		/// <summary>
		/// Perfmorm post-install actions. (ex. Registering a COM server.) (Optional.)
		/// </summary>
		protected virtual void PostInstallAction()
		{ }

		public void CheckUpdate()
		{
			GetInstalledVersion();
			GetAvailableVersion();

			State = UpdaterState.Checked;

			OnUpdateCheckFinished();
		}

		public void Execute(Stream updateDataStream)
		{
			if (State < UpdaterState.Checked)
				throw new InvalidOperationException("Trying to execute update before update check.");

			if (State == UpdaterState.Finished)
				throw new InvalidOperationException("Update was already performed.");

			if (PerformPreInstall)
			{
				OnPerformingPreInstallActions();
				PreInstallAction();
			}

			OnStartingInstall();

			try
			{
				using (updateDataStream)
					Install(updateDataStream);
			}
			catch (UnauthorizedAccessException x)
			{
				throw new UpdaterException("Could not overwrite old version. Installation may be in an invalid state. Close any open relevant open programs and try again.", x);
			}
			catch (IOException x)
			{
				throw new UpdaterException(x.Message, x);
			}

			if (PerformPostInstall)
			{
				OnPerformingPostInstallActions();
				PostInstallAction();
			}

			State = UpdaterState.Finished;
			OnUpdateFinished();
		}

		#region Events
		public event EventHandler<UpdateCheckFinishedEventArgs> UpdateCheckFinished;
		public event EventHandler PerformingPreInstallActions;
		public event EventHandler StartingInstall;
		public event EventHandler PerformingPostInstallActions;
		public event EventHandler UpdateFinished;

		protected virtual void OnUpdateCheckFinished()
		{
			UpdateCheckFinished?.Invoke(this, new UpdateCheckFinishedEventArgs(InstalledVersion, AvailableVersion));
		}

		protected virtual void OnPerformingPreInstallActions()
		{
			PerformingPreInstallActions?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnStartingInstall()
		{
			StartingInstall?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnPerformingPostInstallActions()
		{
			PerformingPostInstallActions?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnUpdateFinished()
		{
			UpdateFinished?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
