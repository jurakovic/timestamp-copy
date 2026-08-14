using System.Globalization;

namespace TimestampCopy.Core;

public static class Fs
{
	/// <summary>
	/// One code path for files and folders, matching <c>Get-Item</c> in TimestampCopy.ps1.
	/// A <c>.lnk</c> is a file, so it takes the <see cref="FileInfo"/> branch.
	/// </summary>
	public static FileSystemInfo GetInfo(string path) =>
		Directory.Exists(path) ? new DirectoryInfo(path) : new FileInfo(path);

	public static bool Exists(string path) =>
		File.Exists(path) || Directory.Exists(path);

	public static void GuardPathExists(string path)
	{
		if (!Exists(path))
			throw new GuardException($"Path '{path}' does not exist.");
	}

	public static string Format(DateTime value) =>
		value.ToString(Constants.DateTimeFormat, CultureInfo.InvariantCulture);

	public static DateTime Parse(string value) =>
		DateTime.ParseExact(value, Constants.DateTimeFormat, CultureInfo.InvariantCulture);

	public static bool TryParse(string value, out DateTime result) =>
		DateTime.TryParseExact(value, Constants.DateTimeFormat, CultureInfo.InvariantCulture,
			DateTimeStyles.None, out result);
}
