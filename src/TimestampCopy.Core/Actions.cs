namespace TimestampCopy.Core;

public static class Actions
{
	public static void Copy(string path, bool quiet)
	{
		Fs.GuardPathExists(path);

		FileSystemInfo item = Fs.GetInfo(path);
		string dc = Fs.Format(item.CreationTime);
		string dm = Fs.Format(item.LastWriteTime);

		Clip.Write(Constants.ClipPath, $"{dc}\n{dm}");

		if (quiet)
			return;

		Ui.WriteLine($"File/Folder:   {path}");
		Ui.WriteLine("---");
		Ui.WriteLine($"Date Created:  {dc}");
		Ui.WriteLine($"Date Modified: {dm}");
		Ui.WriteLine("---");
		Ui.WriteLine("Timestamps copied", ConsoleColor.Green);
	}
}
