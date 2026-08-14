namespace TimestampCopy.Core;

/// <summary>
/// The install/uninstall menu shown when there are no arguments, as in <c>Show-Menu</c>.
/// </summary>
public static class Menu
{
	public static int Show()
	{
		while (true)
		{
			Clear();
			Ui.WriteLine();
			Ui.WriteLine($"  Timestamp Copy ({Constants.Version})    ");
			Ui.WriteLine("                                ");
			Ui.WriteLine("  [i] Install                   ");
			Ui.WriteLine("  [b] Install (Background Mode) ");
			Ui.WriteLine("  [u] Uninstall                 ");
			Ui.WriteLine("  [h] Help                      ");
			Ui.WriteLine("                                ");
			Ui.WriteLine("  [q] Quit                      ");
			Ui.WriteLine();
			Ui.Write("Choose option: ");

			string? option = Console.ReadLine();

			// EOF - stdin was redirected and has run out. Looping would spin forever.
			if (option is null)
				return 0;

			Clear();

			switch (option.Trim().ToLowerInvariant())
			{
				case "i": ContextMenu.Install(ScriptMode.Standalone); break;
				case "b": ContextMenu.Install(ScriptMode.Background); break;
				case "u": ContextMenu.Uninstall(); break;
				case "h": Help.Show(); break;
				case "q": return 0;
				default: Ui.WriteLine($"Unknown option: {option}"); break;
			}

			// The menu only ever runs in its own window, so it always pauses
			Ui.Pause(ScriptMode.Standalone, "continue");
		}
	}

	private static void Clear()
	{
		if (!Console.IsOutputRedirected)
			Console.Clear();
	}
}
