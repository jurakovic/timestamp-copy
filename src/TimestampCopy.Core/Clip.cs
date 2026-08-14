using System.Text;

namespace TimestampCopy.Core;

/// <summary>
/// The on-disk timestamp clipboard: base64(utf8(...)) with LF-separated fields.
/// Byte-compatible with <c>Set-Clipboard-Content</c> / <c>Get-Clipboard-Content</c> in
/// TimestampCopy.ps1 so the script and the exe can be used interchangeably.
/// </summary>
public static class Clip
{
	public static void Write(string path, string value)
	{
		string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

		string? dir = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(dir))
			Directory.CreateDirectory(dir);

		// PowerShell's Set-Content appends a trailing newline; match it for byte parity.
		// The payload is base64, i.e. pure ASCII, so the encoding never adds a BOM.
		File.WriteAllText(path, encoded + Environment.NewLine, new UTF8Encoding(false));
	}

	public static string[] Read(string path)
	{
		string encoded = File.ReadAllText(path).Trim();
		string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
		return decoded.Split('\n');
	}

	public static void WriteTimestamps(string dateCreated, string dateModified) =>
		Write(Constants.ClipPath, $"{dateCreated}\n{dateModified}");

	public static void WriteUndo(string path, string dateCreated, string dateModified) =>
		Write(Constants.UndoPath, $"{path}\n{dateCreated}\n{dateModified}");

	/// <summary>
	/// Reads the two copied timestamps, rejecting a missing or unusable clipboard the same way
	/// <c>Guard-Clipboard</c> does. The trailing spaces in the "empty" message are the script's,
	/// kept so the Background-mode message box stays the same width.
	/// </summary>
	public static string[] ReadTimestamps()
	{
		const string empty = "Timestamps clipboard empty. Copy new timestamps.    ";
		const string corrupted = "Timestamps clipboard corrupted. Copy new timestamps.";

		return ReadGuarded(Constants.ClipPath, expectedCount: 2, firstTimestamp: 0, empty, corrupted);
	}

	private static string[] ReadGuarded(string path, int expectedCount, int firstTimestamp,
		string emptyMessage, string corruptedMessage)
	{
		if (!File.Exists(path))
			throw new GuardException(emptyMessage);

		string[] values;
		try
		{
			values = Read(path);
		}
		catch (FormatException)
		{
			throw new GuardException(corruptedMessage);
		}

		if (values.Length != expectedCount)
			throw new GuardException(corruptedMessage);

		for (int i = firstTimestamp; i < values.Length; i++)
		{
			if (!Fs.TryParse(values[i], out _))
				throw new GuardException(corruptedMessage);
		}

		return values;
	}
}
