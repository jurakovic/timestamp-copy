using TimestampCopy.Core;

namespace TimestampCopy.Cli;

/// <summary>
/// Console entry point: Terminal and Standalone modes.
/// Background mode gets its own WinExe (tscpw) so there is no console to flash.
/// </summary>
internal static class Program
{
	private const int ExitSuccess = 0;
	private const int ExitGuard = 1;
	private const int ExitNotImplemented = 2;

	private static int Main(string[] argv)
	{
		Args args = Args.Parse(argv);

		try
		{
			return Run(args);
		}
		catch (GuardException ex)
		{
			return Guard(args, ex.Message);
		}
		catch (Exception ex)
		{
			// Same shape as the script's try/catch around the timestamp write: report the
			// message, not a stack trace.
			return Guard(args, ex.Message);
		}
	}

	private static int Run(Args args)
	{
		if (args.Error is not null)
			throw new GuardException(args.Error);

		if (args.Help)
		{
			Help.Show();
			return ExitSuccess;
		}

		if (args.Version)
		{
			Ui.WriteLine(Constants.Version);
			return ExitSuccess;
		}

		if (args.Copy is string copy)
			return Run(args, () => Actions.Copy(copy, args.Options));

		if (args.Paste is string paste)
			return Run(args, () => Actions.Paste(paste, args.Options));

		if (args.PasteDateCreated is string pasteDateCreated)
			return Run(args, () => Actions.PasteDateCreated(pasteDateCreated, args.Options));

		if (args.PasteDateModified is string pasteDateModified)
			return Run(args, () => Actions.PasteDateModified(pasteDateModified, args.Options));

		// Undo and install/uninstall are still handled by TimestampCopy.ps1.
		return NotImplemented(args);
	}

	private static int Run(Args args, Action action)
	{
		action();
		Ui.Pause(args.Mode);
		return ExitSuccess;
	}

	private static int NotImplemented(Args args)
	{
		string what = args switch
		{
			{ Install: true } => "-Install (-i)",
			{ InstallBackgroundMode: true } => "-InstallBackgroundMode (-b)",
			{ Uninstall: true } => "-Uninstall (-u)",
			{ Undo: true } => "-Undo (-z)",
			_ => "the interactive menu",
		};

		if (!args.Quiet)
		{
			Ui.WriteLine($"{what} is not implemented yet. Use TimestampCopy.ps1.", ConsoleColor.Red);
			Ui.Pause(args.Mode);
		}

		return ExitNotImplemented;
	}

	private static int Guard(Args args, string message)
	{
		if (!args.Quiet)
		{
			Ui.WriteLine(message, ConsoleColor.Red);
			Ui.Pause(args.Mode);
		}

		return ExitGuard;
	}
}
