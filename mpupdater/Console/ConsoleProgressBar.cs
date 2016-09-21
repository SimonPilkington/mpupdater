using System;

namespace mpupdater
{
	public class ConsoleProgressBar
	{
		private const char PROGRESS_CHARACTER = '=';
		private const int BAR_SIZE = 25;

		private const ConsoleColor COMPLETE_COLOR = ConsoleColor.Green;
		private const ConsoleColor REMAINING_COLOR = ConsoleColor.DarkGreen;

		private int consoleLeft, consoleTop; // location of the progress bar in the console

		public static ConsoleProgressBar Create(string leadingString)
		{
			Console.Write(leadingString);
			var instance = new ConsoleProgressBar(Console.CursorLeft, Console.CursorTop);
			Console.WriteLine();

			return instance;
		}

		private ConsoleProgressBar(int _consoleLeft, int _consoleTop)
		{
			consoleLeft = _consoleLeft;
			consoleTop = _consoleTop;
		}

		public void Draw(double percentage)
		{
			int oldLeft = Console.CursorLeft;
			int oldTop = Console.CursorTop;

			try
			{
				Console.SetCursorPosition(consoleLeft, consoleTop);
				Console.Write('[');

				int chars = (int)Math.Floor(percentage / (100 / (double)BAR_SIZE));

				Console.ForegroundColor = COMPLETE_COLOR;
				Console.Write(new string(PROGRESS_CHARACTER, chars));

				Console.ForegroundColor = REMAINING_COLOR;
				Console.Write(new string(PROGRESS_CHARACTER, BAR_SIZE - chars));

				Console.ResetColor();
				Console.Write(']');

				Console.Write(" {0:F2}%", percentage);
			}
			finally
			{
				Console.SetCursorPosition(oldLeft, oldTop);
			}
		}
	}
}
