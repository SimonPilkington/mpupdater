using System;
using System.IO;

namespace mpupdater
{
	/// <summary>
	/// Wrapper class for registrable COM modules.
	/// </summary>
	public class RegSvr : IDisposable
	{
		private bool disposed = false;
		
		private IntPtr libHandle = IntPtr.Zero;
		private IntPtr regAddr;
		private IntPtr unregAddr;

		private delegate int DllRegFuncDelegate();

		/// <summary>
		/// Initialises a new instance of the RegSvr class.
		/// </summary>
		/// <exception cref="ServerRegException"></exception>
		/// <exception cref="IOException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <param name="libPath">Path to the registrable module.</param>
		
		private RegSvr(string libPath)
		{
			if (libPath == null)
				throw new ArgumentNullException("libPath");

			// some libraries (madVR) try to load other libraries, which can fail unless we switch the current directory
			string oldCurrentDir = Directory.GetCurrentDirectory();
			try
			{
				Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(libPath)));
				libHandle = NativeMethods.LoadLibrary(Path.GetFileName(libPath));
			}
			finally
			{
				Directory.SetCurrentDirectory(oldCurrentDir);
			}

			if (libHandle == IntPtr.Zero)
				throw new ServerRegException("Could not load " + libPath);

			regAddr = NativeMethods.GetProcAddress(libHandle, "DllRegisterServer");
			unregAddr = NativeMethods.GetProcAddress(libHandle, "DllUnregisterServer");

			if (regAddr == IntPtr.Zero || unregAddr == IntPtr.Zero)
			{
				if (libHandle != IntPtr.Zero)
				{
					NativeMethods.FreeLibrary(libHandle);
					libHandle = IntPtr.Zero;
				}

				throw new ServerRegException(libPath + " is not a registrable COM module.");
			}
		}

		public void Register()
		{
			if (disposed)
				throw new ObjectDisposedException("RegSvr");

			var func = NativeMethods.GetDllFuncDelegate<DllRegFuncDelegate>(regAddr);

			int result = func();
			if (result != NativeMethods.S_OK)
				throw new ServerRegException("Server registration failed with HRESULT 0x" + result.ToString("X"));
		}

		public void Unregister()
		{
			if (disposed)
				throw new ObjectDisposedException("RegSvr");

			var func = NativeMethods.GetDllFuncDelegate<DllRegFuncDelegate>(unregAddr);

			int result = func();
			if (result != NativeMethods.S_OK)
				throw new ServerRegException("Server unregistration failed with HRESULT 0x" + result.ToString("X"));
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (libHandle != IntPtr.Zero)
					NativeMethods.FreeLibrary(libHandle);

				disposed = true;
			}
		}

		~RegSvr()
		{
			Dispose(false);
		}

		public static void RegisterServer(string dllFileName)
		{
			using (var svr = new RegSvr(dllFileName))
				svr.Register();
		}

		public static void UnregisterServer(string dllFileName)
		{
			using (var svr = new RegSvr(dllFileName))
				svr.Unregister();
		}
	}
}
