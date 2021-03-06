﻿using System;
using System.IO;
using System.IO.Compression;

namespace mpupdater
{
	public abstract class ComFilterUpdater : Updater
	{
		protected abstract string filterDll { get; }
		protected abstract string filterPath { get; }

		private void Unregister()
		{
#if !DEBUG_NET
			try
			{
				RegSvr.UnregisterServer(Path.Combine(filterPath, filterDll));
			}
			catch (Exception x)
			{
				if (x is IOException)
					throw new UpdaterException($"Failed to open {filterDll}.");

				if (x is ArgumentException || x is ServerRegException)
					throw new UpdaterException(x.Message);

				throw;
			}
#endif
		}

		private void Register()
		{
#if !DEBUG_NET
			try
			{
				RegSvr.RegisterServer(Path.Combine(filterPath, filterDll));
			}
			catch (Exception x)
			{
				if (x is IOException)
					throw new UpdaterException($"Failed to open {filterDll}.", x);

				if (x is ArgumentException || x is ServerRegException)
					throw new UpdaterException(x.Message, x);

				throw;
			}
#endif
		}

		protected override bool PerformPreInstall => InstalledVersion.Installed;
		protected override void PreInstallAction()
		{
			Unregister();
		}

		protected override bool PerformPostInstall => true;
		protected override void PostInstallAction()
		{
			Register();
		}

		protected override void Install(Stream updateStream)
		{
			string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			try
			{
				using (var extractor = new ZipArchive(updateStream))
					extractor.ExtractToDirectory(tempDir);

				IOExt.MoveDirWithOverwrite(tempDir, filterPath);
			}
			finally
			{
				if (Directory.Exists(tempDir))
					Directory.Delete(tempDir, true);
			}
		}
	}
}
