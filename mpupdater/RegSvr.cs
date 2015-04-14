using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace mpupdater
{
	public class RegSvr : IDisposable
	{
		const int S_OK = 0;

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool FreeLibrary(IntPtr hModule);

		private IntPtr libHandle;
		private IntPtr regAddr;
		private IntPtr unregAddr;

		private delegate int DllFuncPointerDelegate();

		public RegSvr(string libPath)
		{
			libHandle = LoadLibrary(libPath);

			if (libHandle == IntPtr.Zero)
				throw new System.IO.IOException("Could not load " + libPath);

			regAddr = GetProcAddress(libHandle, "DllRegisterServer");
			unregAddr = GetProcAddress(libHandle, "DllUnregisterServer");

			if (regAddr == IntPtr.Zero || unregAddr == IntPtr.Zero)
				throw new ArgumentException(libPath + " is not a registrable COM DLL");
		}

		public void Register()
		{
			DllFuncPointerDelegate func = (DllFuncPointerDelegate)Marshal.GetDelegateForFunctionPointer(regAddr, typeof(DllFuncPointerDelegate));

			int result = func();
			if (result != S_OK)
				throw new ServerRegException("Server registration failed with code 0x" + result.ToString("X"));
			
		}

		public void Unregister()
		{
			DllFuncPointerDelegate func = (DllFuncPointerDelegate)Marshal.GetDelegateForFunctionPointer(unregAddr, typeof(DllFuncPointerDelegate));

			int result = func();
			if (result != S_OK)
				throw new ServerRegException("Server unregistration failed with code 0x" + result.ToString("X"));
		}

		public void Dispose()
		{
			FreeLibrary(libHandle);
		}
	}
}
