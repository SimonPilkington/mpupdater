using System;

namespace mpupdater
{
	public static class ConsoleExt
	{
		/// <summary>
		/// Used to avoid race conditions during console access.
		/// </summary>
		public static readonly object ConsoleLock = new object();

		public static void SafeWriteLine(string text)
		{
			lock (ConsoleLock)
				Console.WriteLine(text);
		}

		public static void SafeWrite(string text)
		{
			lock (ConsoleLock)
				Console.Write(text);
		}
	}
}
