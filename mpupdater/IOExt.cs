using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;
using System.ComponentModel;

namespace mpupdater
{
	public static class IOExt
	{
		public static void GetFileVersion(string path, out int major, out int minor, out int priv, out int build)
		{
			FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
			major = info.FileMajorPart;
			minor = info.FileMinorPart;
			priv = info.FilePrivatePart;
			build = info.FileBuildPart;
		}

		public static void ExtractSevenZip(string archivePath, string outPath)
		{
			using (var sevenZip = new Process())
			{
				sevenZip.StartInfo.FileName = "7zr.exe";
				sevenZip.StartInfo.Arguments = "x  -y -o" + outPath + " " + archivePath;
				sevenZip.StartInfo.CreateNoWindow = false;
				sevenZip.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

				try
				{
					sevenZip.Start();
				}
				catch (Win32Exception)
				{
					throw new SevenZipException("7zip not found; need 7zr.exe in working directory.");
				}

				if (!sevenZip.WaitForExit(30 * 1000))
				{
					sevenZip.Kill();
					throw new SevenZipException("7zip took over 30 seconds.");
				}

				if (sevenZip.ExitCode != 0)
					throw new SevenZipException("Something went wrong with 7zip.");
			}
		}

		public static void ExtractSevenZip(string archivePath)
		{
			ExtractSevenZip(archivePath, ".");
		}

		public static int RunProcess(string path)
		{
			using (var proc = new Process())
			{
				proc.StartInfo.FileName = path;
				proc.StartInfo.CreateNoWindow = false;
				proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

				try
				{
					proc.Start();
				}
				catch (Win32Exception)
				{
					throw new UpdateCheckException("Failed to run " + path);
				}

				proc.WaitForExit();
				return proc.ExitCode;
			}
		}

		public static void MoveDirWithOverwrite(string srcPath, string dstPath)
		{
			if (!Directory.Exists(srcPath))
				throw new ArgumentException("srcPath: doesn't exist or not a directory.");

			if (!Directory.Exists(dstPath))
				Directory.CreateDirectory(dstPath);

			var subDirs = Directory.EnumerateDirectories(srcPath);

			foreach (var dir in subDirs)
			{
				MoveDirWithOverwrite(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
				
				//Console.WriteLine("recursively move " + dir + " to " + Path.Combine(dstPath, Path.GetFileName(dir)));
			}

			var files = Directory.EnumerateFiles(srcPath);

			foreach (var file in files)
			{
				string dst = Path.Combine(dstPath, Path.GetFileName(file));

				if (File.Exists(dst))
					File.Delete(dst);

				File.Move(file, dst);

				//Console.WriteLine("move " + file + " to " + dst);
			}

			Directory.Delete(srcPath);
		}
	}
}
