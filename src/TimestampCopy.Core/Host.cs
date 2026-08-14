namespace TimestampCopy.Core;

/// <summary>
/// The whole command line dispatch, shared by both executables.
/// <para>
/// It lives here rather than in each <c>Main</c> for a build reason as much as a tidiness one:
/// tscp and tscpw publish into one folder and share a single copy of the trimmed runtime, so
/// whichever is published second overwrites the other's framework assemblies. Driving both from
/// this one method gives them the same static closure, and therefore the same trimmer output.
/// </para>
/// </summary>
public static class Host
{
	private const int ExitSuccess = 0;
	private const int ExitGuard = 1;
	private const int ExitNotSupported = 2;

	/// <param name="background">
	/// True for tscpw: no console exists, so nothing is printed, guard messages become message
	/// boxes, and changes are applied without a confirmation prompt.
	/// </param>
	public static int Run(string[] argv, bool background)
	{
		Args args = Args.Parse(argv, background ? ScriptMode.Background : ScriptMode.Terminal);

		Options options = background
			? new Options(Quiet: true, args.SkipConfirm, ScriptMode.Background)
			: args.Options;

		try
		{
			return Execute(args, options, background);
		}
		catch (GuardException ex)
		{
			return Report(ex.Message, options, background, ExitGuard);
		}
		catch (Exception ex)
		{
			// Same shape as the script's try/catch around the timestamp write: report the
			// message, not a stack trace.
			return Report(ex.Message, options, background, ExitGuard);
		}
	}

	private static int Execute(Args args, Options options, bool background)
	{
		if (args.Error is not null)
			throw new GuardException(args.Error);

		if (!background)
		{
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

			// Install and Uninstall do not pause on success, matching the script
			if (args.Install)
			{
				ContextMenu.Install(ScriptMode.Standalone);
				return ExitSuccess;
			}

			if (args.InstallBackgroundMode)
			{
				ContextMenu.Install(ScriptMode.Background);
				return ExitSuccess;
			}

			if (args.Uninstall)
			{
				ContextMenu.Uninstall();
				return ExitSuccess;
			}
		}

		if (args.Copy is string copy)
			return Act(() => Actions.Copy(copy, options), options, background);

		if (args.Paste is string paste)
			return Act(() => Actions.Paste(paste, options), options, background);

		if (args.PasteDateCreated is string pasteDateCreated)
			return Act(() => Actions.PasteDateCreated(pasteDateCreated, options), options, background);

		if (args.PasteDateModified is string pasteDateModified)
			return Act(() => Actions.PasteDateModified(pasteDateModified, options), options, background);

		if (args.Undo)
			return Act(() => Actions.Undo(options), options, background);

		// Nothing was asked for. On a console that means the install/uninstall menu; tscpw has
		// no console to show it on.
		if (!background)
			return Menu.Show();

		return NotSupported(args, options, background);
	}

	private static int Act(Action action, Options options, bool background)
	{
		action();

		if (!background)
			Ui.Pause(options.Mode);

		return ExitSuccess;
	}

	/// <summary>
	/// Only reachable from tscpw: everything a console can do, a console does. tscpw exists to
	/// run the five context menu actions without a window, and nothing else.
	/// </summary>
	private static int NotSupported(Args args, Options options, bool background)
	{
		string what = args switch
		{
			{ Install: true } => "-Install (-i)",
			{ InstallBackgroundMode: true } => "-InstallBackgroundMode (-b)",
			{ Uninstall: true } => "-Uninstall (-u)",
			{ Help: true } => "-Help (-h)",
			{ Version: true } => "-Version (-v)",
			_ => "No action given.",
		};

		return Report($"{what} is not available in background mode. Use tscp.exe.", options,
			background, ExitNotSupported);
	}

	private static int Report(string message, Options options, bool background, int exitCode)
	{
		// Background always shows the message, even under -q: a silent failure there is
		// invisible. Matches Show-Guard-Message, which tests the mode before the quiet flag.
		if (background)
			MessageBox.Show(message);
		else if (!options.Quiet)
		{
			Ui.WriteLine(message, ConsoleColor.Red);
			Ui.Pause(options.Mode);
		}

		return exitCode;
	}
}
