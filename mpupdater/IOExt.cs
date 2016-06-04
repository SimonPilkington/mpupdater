using System;
using System.IO;

namespace mpupdater
{
	public static class IOExt
	{
		public static void MoveDirWithOverwrite(string srcPath, string dstPath)
		{
			if (!Directory.Exists(srcPath))
				throw new ArgumentException("srcPath: doesn't exist or not a directory.", "srcPath");

			if (Path.GetFullPath(srcPath) == Path.GetFullPath(dstPath)) // this shouldn't happen
				throw new ArgumentException("srcPath and dstPath must not be equal."); 

			if (!Directory.Exists(dstPath))
				Directory.CreateDirectory(dstPath);

			var subDirs = Directory.EnumerateDirectories(srcPath);

			// move subdirs recursively
			foreach (var dir in subDirs)
			{
				string subDirPath = Path.Combine(dstPath, Path.GetFileName(dir));
				MoveDirWithOverwrite(dir, subDirPath);
			}

			var files = Directory.EnumerateFiles(srcPath);

			foreach (var file in files)
			{
				string dst = Path.Combine(dstPath, Path.GetFileName(file));

				if (File.Exists(dst))
					File.Delete(dst);

				File.Move(file, dst);
			}

			Directory.Delete(srcPath); // remove the now empty source directory
		}
	}
}
