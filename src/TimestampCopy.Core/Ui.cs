namespace TimestampCopy.Core;

/// <summary>
/// Console output, mirroring <c>Write-Host</c> (colour is restored after every write) and
/// <c>Pause-Script</c> from TimestampCopy.ps1.
/// </summary>
public static class Ui
{
	public static void Write(string text, ConsoleColor? color = null)
	{
		if (color is null)
		{
			Console.Write(text);
			return;
		}

		ConsoleColor previous = Console.ForegroundColor;
		Console.ForegroundColor = color.Value;
		try
		{
			Console.Write(text);
		}
		finally
		{
			Console.ForegroundColor = previous;
		}
	}

	public static void WriteLine(string text = "", ConsoleColor? color = null)
	{
		Write(text, color);
		Console.WriteLine();
	}

	public static void Pause(ScriptMode mode, string option = "exit")
	{
		// There is nothing to keep open when the caller redirected stdin (a pipe, a test run),
		// and ReadKey would throw. Explorer-launched runs always have a real console.
		if (mode != ScriptMode.Standalone || Console.IsInputRedirected)
			return;

		Write($"Press any key to {option}...");
		Console.ReadKey(intercept: true);
	}

	/// <summary>
	/// Prints the old value, then the new value with the differing components in green.
	/// Both values are in <see cref="Constants.DateTimeFormat"/>.
	/// </summary>
	public static void HighlightDiff(string label, string old, string @new)
	{
		WriteLine($"{label} {old} (old)");
		Write($"{label} ");

		char[] separators = ['-', ' ', ':'];
		string[] oldParts = old.Split(separators);
		string[] newParts = @new.Split(separators);

		string[] joiners = ["-", "-", " ", ":", ":"];
		bool changed = false;

		for (int i = 0; i < newParts.Length; i++)
		{
			bool partChanged = i >= oldParts.Length || oldParts[i] != newParts[i];
			changed |= partChanged;

			Write(newParts[i], partChanged ? ConsoleColor.Green : null);

			if (i < joiners.Length && i < newParts.Length - 1)
				Write(joiners[i]);
		}

		WriteLine(" (new)", changed ? ConsoleColor.Green : null);
	}
}
