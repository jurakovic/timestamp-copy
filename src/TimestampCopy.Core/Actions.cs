namespace TimestampCopy.Core;

public static class Actions
{
	public static void Copy(string path, Options options)
	{
		Fs.GuardPathExists(path);

		FileSystemInfo item = Fs.GetInfo(path);
		string dc = Fs.Format(item.CreationTime);
		string dm = Fs.Format(item.LastWriteTime);

		Clip.WriteTimestamps(dc, dm);

		if (options.Quiet)
			return;

		Ui.WriteLine($"File/Folder:   {path}");
		Ui.WriteLine("---");
		Ui.WriteLine($"Date Created:  {dc}");
		Ui.WriteLine($"Date Modified: {dm}");
		Ui.WriteLine("---");
		Ui.WriteLine("Timestamps copied", ConsoleColor.Green);
	}

	public static void Paste(string path, Options options)
	{
		(string dcOld, string dmOld, string[] clip) = Prepare(path);
		Apply(path, dcOld, clip[0], dmOld, clip[1], options);
	}

	public static void PasteDateCreated(string path, Options options)
	{
		(string dcOld, string dmOld, string[] clip) = Prepare(path);
		// Date Modified is rewritten with its current value, so that Windows File Explorer
		// still sees a change and refreshes.
		Apply(path, dcOld, clip[0], dmOld, dmOld, options);
	}

	public static void PasteDateModified(string path, Options options)
	{
		(string dcOld, string dmOld, string[] clip) = Prepare(path);
		Apply(path, dcOld, dcOld, dmOld, clip[1], options);
	}

	private static (string DateCreated, string DateModified, string[] Clip) Prepare(string path)
	{
		Fs.GuardPathExists(path);
		string[] clip = Clip.ReadTimestamps();

		FileSystemInfo item = Fs.GetInfo(path);
		return (Fs.Format(item.CreationTime), Fs.Format(item.LastWriteTime), clip);
	}

	private static void Apply(string path, string dcOld, string dcNew, string dmOld, string dmNew,
		Options options)
	{
		if (!options.Quiet)
		{
			Ui.WriteLine($"File/Folder:   {path}");
			Ui.WriteLine("---");
			Ui.HighlightDiff("Date Created: ", dcOld, dcNew);
			Ui.WriteLine("---");
			Ui.HighlightDiff("Date Modified:", dmOld, dmNew);
			Ui.WriteLine("---");
		}

		if (!options.AutoConfirm && !Ui.Confirm())
		{
			if (!options.Quiet)
				Ui.WriteLine("Canceled", ConsoleColor.Yellow);

			return;
		}

		try
		{
			FileSystemInfo item = Fs.GetInfo(path);
			// Changing both values triggers "Refresh" in Windows File Explorer
			item.CreationTime = Fs.Parse(dcNew);
			item.LastWriteTime = Fs.Parse(dmNew);
		}
		catch (Exception ex)
		{
			if (!options.Quiet)
				Ui.WriteLine("Error", ConsoleColor.Red);

			throw new GuardException(ex.Message);
		}

		// Backup old timestamps. Written only after the change succeeded, so Undo never points
		// at a file that was not touched.
		Clip.WriteUndo(path, dcOld, dmOld);

		if (!options.Quiet)
			Ui.WriteLine("Done", ConsoleColor.Green);
	}
}
