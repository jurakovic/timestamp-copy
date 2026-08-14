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

	/// <summary>
	/// The <c>Read-Host "Apply changes? (y/N)"</c> prompt. Only an exact "y" (any case) applies,
	/// matching the script; anything else, including EOF, cancels.
	/// </summary>
	public static bool Confirm()
	{
		Write("Apply changes? (y/N): ");

		// Windows PowerShell 5.1 prefixes a UTF-8 BOM when it pipes into a native command,
		// which would otherwise turn a scripted "y" into a silent cancel. Nothing else is
		// trimmed: as in the script, anything but a bare "y" cancels.
		string? answer = Console.ReadLine()?.TrimStart('\uFEFF');

		return string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase);
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
