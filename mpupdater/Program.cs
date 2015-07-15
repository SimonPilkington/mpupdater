using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace mpupdater
{
	class Program
	{
		static void DoUpdate(string name, Updater updater)
		{
			Console.WriteLine("=== " + name + " ===");

			try
			{
				updater.Update();
			}
			catch (Exception e)
			{
				if (e is UpdaterException || e is WebException || e is IOException)
					Console.WriteLine("Update failed: " + e.Message);
				else
					throw;
			}
			finally
			{
				Console.WriteLine();
			}
		}

		static void Main(string[] args)
		{
#if WIN64
			Console.WriteLine("Video nonsense updater - version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " x64"+ Environment.NewLine);
#else
			Console.WriteLine("Video nonsense updater - version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " x86"+ Environment.NewLine);
#endif

            Updater[] updaters = new Updater[] {new MediaPlayerUpdater(), new MadVRUpdater(), new SubFilterUpdater()};

			DoUpdate("MPC-HC", updaters[0]);
			DoUpdate("madVR", updaters[1]);
			DoUpdate("XySubFilter", updaters[2]);

			Console.ReadKey();
		}
	}
}
