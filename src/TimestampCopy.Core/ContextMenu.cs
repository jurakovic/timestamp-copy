using System.Security.Principal;
using Microsoft.Win32;

namespace TimestampCopy.Core;

/// <summary>
/// Installs and removes the Explorer context menu entries under HKEY_CLASSES_ROOT, which is
/// what <c>reg.exe add</c> / <c>reg.exe delete</c> did in TimestampCopy.ps1.
/// </summary>
public static class ContextMenu
{
	private static readonly string[] RootKeys =
	[
		@"*\shell\TimestampCopy",
		@"Directory\shell\TimestampCopy",
		@"lnkfile\shell\TimestampCopy",
	];

	private static readonly (string Key, string Label, string Flag, bool Path)[] Items =
	[
		(@"shell\010-Copy", "Copy", "-c", true),
		(@"shell\020-Paste", "Paste", "-p", true),
		(@"shell\030-PasteDateCreated", "Paste \"Date Created\"", "-pc", true),
		(@"shell\040-PasteDateModified", "Paste \"Date Modified\"", "-pm", true),
		(@"shell\050-Undo", "Undo", "-z", false),
	];

	public static void Install(ScriptMode mode)
	{
		GuardAdministrator();

		string suffix = mode == ScriptMode.Background ? " (Background Mode)" : "";
		Ui.WriteLine($"Installing{suffix}...");

		Directory.CreateDirectory(Constants.AppDataPath);

		foreach (string rootKey in RootKeys)
			AddContextMenu(rootKey, mode);

		Ui.WriteLine("Done", ConsoleColor.Green);
	}

	public static void Uninstall()
	{
		GuardAdministrator();

		Ui.WriteLine("Uninstalling...");

		foreach (string rootKey in RootKeys)
			Registry.ClassesRoot.DeleteSubKeyTree(rootKey, throwOnMissingSubKey: false);

		if (Directory.Exists(Constants.AppDataPath))
			Directory.Delete(Constants.AppDataPath, recursive: true);

		Ui.WriteLine("Done", ConsoleColor.Green);
	}

	private static void AddContextMenu(string rootKey, ScriptMode mode)
	{
		using (RegistryKey root = Registry.ClassesRoot.CreateSubKey(rootKey))
		{
			root.SetValue("MUIVerb", "Timestamp Copy");
			root.SetValue("SubCommands", "");
			// The icon is embedded in the executable, so nothing has to sit next to it
			root.SetValue("Icon", Constants.ExePath);
		}

		foreach ((string key, string label, string flag, bool path) in Items)
		{
			using (RegistryKey item = Registry.ClassesRoot.CreateSubKey($@"{rootKey}\{key}"))
				item.SetValue(null, label);

			using RegistryKey command = Registry.ClassesRoot.CreateSubKey($@"{rootKey}\{key}\command");
			command.SetValue(null, Command(mode, flag, path));
		}
	}

	private static string Command(ScriptMode mode, string flag, bool path)
	{
		// Background mode runs tscpw.exe, which has no console and therefore needs neither a
		// headless conhost prefix nor -ScriptMode. Undo is the one item that takes no path.
		string argument = path ? " \"%1\"" : "";

		return mode == ScriptMode.Background
			? $"\"{Constants.ExePathBackground}\" {flag}{argument}"
			: $"\"{Constants.ExePath}\" -m {mode} {flag}{argument}";
	}

	private static void GuardAdministrator()
	{
		using WindowsIdentity identity = WindowsIdentity.GetCurrent();

		if (!new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))
			throw new GuardException("Administrator privileges required. Run as Administrator.");
	}
}
