namespace TimestampCopy.Core;

public static class Constants
{
	public const string Homepage = "https://github.com/jurakovic/timestamp-copy";
	public const string Version = "3.0.0";

	// Compatibility contract with TimestampCopy.ps1 ($datetimeFormat, $appdataPath,
	// $clipPath, $undoPath). Both implementations read and write the same files during
	// the migration, so these must not drift. See PLAN.md "Compatibility contract".
	public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

	public static string AppDataPath { get; } = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"TimestampCopy");

	public static string ClipPath { get; } = Path.Combine(AppDataPath, "clip");

	public static string UndoPath { get; } = Path.Combine(AppDataPath, "clip-undo");
}
