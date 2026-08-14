namespace TimestampCopy.Core;

public static class Help
{
	public static void Show()
	{
		Ui.WriteLine($"TimestampCopy v{Constants.Version}");
		Ui.WriteLine();
		Ui.WriteLine("Parameters:");
		Ui.WriteLine("  -Help (-h)                       Print help.");
		Ui.WriteLine("  -Version (-v)                    Print the current version.");
		Ui.WriteLine("  -Install (-i)                    Install the context menu entries in Standalone Mode.");
		Ui.WriteLine("  -InstallBackgroundMode (-b)      Install the context menu entries in Background Mode (runs without a terminal window).");
		Ui.WriteLine("  -Uninstall (-u)                  Uninstall the context menu entries and remove related data.");
		Ui.WriteLine("  -Copy (-c) <path>                Copy timestamps of the specified file or folder to the clipboard.");
		Ui.WriteLine("  -Paste (-p) <path>               Paste the copied timestamps to the specified file or folder.");
		Ui.WriteLine("  -PasteDateCreated (-pc) <path>   Paste only the copied Date Created timestamp to the specified file or folder.");
		Ui.WriteLine("  -PasteDateModified (-pm) <path>  Paste only the copied Date Modified timestamp to the specified file or folder.");
		Ui.WriteLine("  -Undo (-z)                       Restore the previous timestamps of the last modified file or folder.");
		Ui.WriteLine("  -Quiet (-q)                      Suppress output messages. After run check the exit code.");
		Ui.WriteLine("  -SkipConfirm (-y)                Skip confirmation prompts when applying changes.");
		Ui.WriteLine("  *none*                           Show the install/uninstall menu.");
		Ui.WriteLine();
		Ui.WriteLine("Some examples:");
		Ui.WriteLine("# Install the context menu entries");
		Ui.WriteLine("tscp.exe -i");
		Ui.WriteLine();
		Ui.WriteLine("# Copy timestamps");
		Ui.WriteLine("tscp.exe -c \"C:\\Foo.txt\"");
		Ui.WriteLine();
		Ui.WriteLine("# Paste timestamps");
		Ui.WriteLine("tscp.exe -p \"D:\\Bar.txt\"");
		Ui.WriteLine();
		Ui.WriteLine("# Paste timestamps without output messages (confirm prompt still shown)");
		Ui.WriteLine("tscp.exe -p \"D:\\Bar.txt\" -q");
		Ui.WriteLine();
		Ui.WriteLine("# Paste timestamps without output messages and confirm prompt");
		Ui.WriteLine("tscp.exe -p \"D:\\Bar.txt\" -q -y");
		Ui.WriteLine();
		Ui.WriteLine("# Paste Date Created");
		Ui.WriteLine("tscp.exe -pc \"D:\\Bar.txt\"");
		Ui.WriteLine();
		Ui.WriteLine("# Paste Date Modified");
		Ui.WriteLine("tscp.exe -pm \"D:\\Bar.txt\"");
		Ui.WriteLine();
		Ui.WriteLine("# Undo");
		Ui.WriteLine("tscp.exe -z");
		Ui.WriteLine();
		Ui.WriteLine($"For more information, visit {Constants.Homepage}");
	}
}
