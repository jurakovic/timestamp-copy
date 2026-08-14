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

		if (args.Copy is not null)
		{
			Actions.Copy(args.Copy, args.Quiet);
			Ui.Pause(args.Mode);
			return ExitSuccess;
		}

		// Step 1 ships -c only; the remaining actions are still handled by TimestampCopy.ps1.
		// See PLAN.md "Steps".
		return NotImplemented(args);
	}

	private static int NotImplemented(Args args)
	{
		string what = args switch
		{
			{ Install: true } => "-Install (-i)",
			{ InstallBackgroundMode: true } => "-InstallBackgroundMode (-b)",
			{ Uninstall: true } => "-Uninstall (-u)",
			{ Paste: not null } => "-Paste (-p)",
			{ PasteDateCreated: not null } => "-PasteDateCreated (-pc)",
			{ PasteDateModified: not null } => "-PasteDateModified (-pm)",
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
