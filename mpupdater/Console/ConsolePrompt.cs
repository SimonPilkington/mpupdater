using System;

namespace mpupdater
{
	public class ConsolePrompt
	{
		public static bool Create(string prompt, bool defaultValue = true)
		{
			try
			{
				while (true)
				{
					Console.Write(prompt);
					Console.Write(" (");
					Console.Write(defaultValue ? "[y]" : "y");
					Console.Write('/');
					Console.Write(!defaultValue ? "[n]" : "n");
					Console.Write(") ");

					ConsoleKey key = Console.ReadKey().Key;

					switch (key)
					{
						case ConsoleKey.Y:
							return true;
						case ConsoleKey.N:
							return false;
						case ConsoleKey.Enter:
							return defaultValue;
					}

					Console.Write('\n');
				}
			}
			finally
			{
				Console.WriteLine();
			}
		}
	}
}
