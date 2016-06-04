using System;
using System.Runtime.InteropServices;

namespace mpupdater
{
	internal static class NativeMethods
	{
		public const int S_OK = 0;
		
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)] // no unicode version of GetProcAddress
		public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName); 

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool FreeLibrary(IntPtr hModule);

		public static T GetDllFuncDelegate<T>(IntPtr funcAddr) where T : class
		{
			return Marshal.GetDelegateForFunctionPointer(funcAddr, typeof(T)) as T;
		}
	}
}
