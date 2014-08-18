using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpupdater
{
	public static class ConsoleExt
	{	
		public static void DrawProgressBar(int perc, int barSize, char progressCharacter)
		{
			Console.CursorVisible = false;
			int left = Console.CursorLeft;
			int chars = (int)Math.Floor(perc / ((decimal)100 / (decimal)barSize));
			string p1 = String.Empty, p2 = String.Empty;

			for (int i = 0; i < chars; i++) p1 += progressCharacter;
			for (int i = 0; i < barSize - chars; i++) p2 += progressCharacter;


			Console.Write('[');
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write(p1);
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.Write(p2);
			Console.ResetColor();
			Console.Write(']');

			Console.Write(" {0}%", perc);
			Console.CursorLeft = left;
		}

		public static void DrawProgressBar(long complete, long maxVal, int barSize, char progressCharacter)
		{
			int perc = (int)(complete / (decimal)maxVal);
			DrawProgressBar(perc, barSize, progressCharacter);
		}
	}
}
