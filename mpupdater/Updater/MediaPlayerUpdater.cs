using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace mpupdater
{
	public sealed class MediaPlayerUpdater : Updater
	{
#if WIN64
		const string MediaPlayerPath = @"MPC-HC64\";
		const string MediaPlayerExecutable = "mpc-hc64.exe";
#else
		const string MediaPlayerPath = @"MPC-HC\";
		const string MediaPlayerExecutable = "mpc-hc.exe";
#endif

		private const string UpdateURL = "https://nightly.mpc-hc.org/";
		protected override string GetUpdateURL
		{
			get
			{
				return UpdateURL;
			}
		}

		protected override string GetVersionURL
		{
			get
			{
				return UpdateURL;
			}
		}

#if WIN64
		protected override string GetVersionRegexPattern
		{
			get
			{
				return @"MPC-HC\.(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?\.x64\.7z";
			}
		}
#else
		protected override string GetVersionRegexPattern
		{
			get
			{
				return @"MPC-HC\.(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?\.x86\.7z";
			}
		}
#endif

        protected override void GetInstalledVersion()
		{
			try
			{
				IOExt.GetFileVersion(Path.Combine(MediaPlayerPath, MediaPlayerExecutable), out InstalledVersion.Major, out InstalledVersion.Minor, out InstalledVersion.Private, out InstalledVersion.Build);
				InstalledVersion.Installed = true;
			}
			catch (FileNotFoundException)
			{ }
		}

		public override void Update()
		{
			if (!CheckUpdate())
				return;

#if WIN64
			string fileName = "MPC-HC." + CurrentVersion + ".x64.7z";
#else
			string fileName = "MPC-HC." + CurrentVersion + ".x86.7z";
#endif

            Console.WriteLine("Downloading update...");
			DownloadUpdateWithProgress(fileName);
						
			Console.WriteLine("Extracting...");

			try
			{
				var ext = new SevenZip.SevenZipExtractor(fileName);
				ext.ExtractArchive(".");

                try
                {
                    IOExt.MoveDirWithOverwrite(Path.GetFileNameWithoutExtension(fileName), MediaPlayerPath);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new UpdateCheckException("Could not overwrite old version. Is the player running?");
                }
                
			}
			finally
			{
				if (File.Exists(fileName))
					File.Delete(fileName);
			}
			
			Console.WriteLine("Done.");
		}

#if false
        public override void Remove()
        {
            Directory.Delete(MediaPlayerPath, true);
        } 
#endif
    }
}
