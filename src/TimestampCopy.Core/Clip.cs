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
}
