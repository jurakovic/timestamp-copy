namespace TimestampCopy.Core;

/// <summary>
/// Hand-rolled parser for the same flags TimestampCopy.ps1 accepts, case-insensitive,
/// in both short (<c>-c</c>) and long (<c>-Copy</c>) form.
/// </summary>
public sealed class Args
{
	public bool Help { get; private set; }
	public bool Version { get; private set; }
	public bool Install { get; private set; }
	public bool InstallBackgroundMode { get; private set; }
	public bool Uninstall { get; private set; }
	public bool Undo { get; private set; }
	public bool Quiet { get; private set; }
	public bool SkipConfirm { get; private set; }

	public string? Copy { get; private set; }
	public string? Paste { get; private set; }
	public string? PasteDateCreated { get; private set; }
	public string? PasteDateModified { get; private set; }

	public ScriptMode Mode { get; private set; }

	/// <summary>
	/// Set when the command line is unusable. Reported as a guard message by the host, so
	/// that <see cref="Mode"/> and <see cref="Quiet"/> parsed so far still decide how it shows.
	/// </summary>
	public string? Error { get; private set; }

	public Options Options => new(Quiet, SkipConfirm, Mode);

	public bool HasAction =>
		Copy is not null || Paste is not null || PasteDateCreated is not null
		|| PasteDateModified is not null || Undo;

	public static Args Parse(string[] argv, ScriptMode defaultMode = ScriptMode.Terminal)
	{
		Args args = new() { Mode = defaultMode };
		args.ParseCore(argv);
		args.Validate();
		return args;
	}

	private void ParseCore(string[] argv)
	{
		for (int i = 0; i < argv.Length; i++)
		{
			string arg = argv[i];

			if (arg.Length < 2 || (arg[0] != '-' && arg[0] != '/'))
			{
				Fail($"Unexpected argument '{arg}'.");
				return;
			}

			string name = arg[1..];

			switch (name.ToLowerInvariant())
			{
				case "h" or "help" or "?": Help = true; break;
				case "v" or "version": Version = true; break;
				case "i" or "install": Install = true; break;
				case "b" or "installbackgroundmode": InstallBackgroundMode = true; break;
				case "u" or "uninstall": Uninstall = true; break;
				case "z" or "undo": Undo = true; break;
				case "q" or "quiet": Quiet = true; break;
				case "y" or "skipconfirm": SkipConfirm = true; break;

				case "c" or "copy": Copy = Value(argv, ref i, arg); break;
				case "p" or "paste": Paste = Value(argv, ref i, arg); break;
				case "pc" or "pastedatecreated": PasteDateCreated = Value(argv, ref i, arg); break;
				case "pm" or "pastedatemodified": PasteDateModified = Value(argv, ref i, arg); break;
				case "m" or "scriptmode": Mode = ParseMode(Value(argv, ref i, arg)); break;

				default:
					Fail($"Unknown parameter '{arg}'.");
					return;
			}

			if (Error is not null)
				return;
		}
	}

	private void Validate()
	{
		if (Error is not null)
			return;

		int install = (Install ? 1 : 0) + (InstallBackgroundMode ? 1 : 0) + (Uninstall ? 1 : 0);
		if (install >= 2)
			Fail("Parameters -Install, -InstallBackgroundMode, -Uninstall cannot be used together.");

		int action = (Copy is not null ? 1 : 0) + (Paste is not null ? 1 : 0)
			+ (PasteDateCreated is not null ? 1 : 0) + (PasteDateModified is not null ? 1 : 0)
			+ (Undo ? 1 : 0);
		if (action >= 2)
			Fail("Parameters -Copy, -Paste, -PasteDateCreated, -PasteDateModified, -Undo cannot be used together.");
	}

	private string Value(string[] argv, ref int i, string arg)
	{
		if (i + 1 >= argv.Length)
		{
			Fail($"Parameter '{arg}' requires a value.");
			return string.Empty;
		}

		return argv[++i];
	}

	private ScriptMode ParseMode(string value)
	{
		if (Error is not null)
			return Mode;

		return value.ToLowerInvariant() switch
		{
			"terminal" => ScriptMode.Terminal,
			"standalone" => ScriptMode.Standalone,
			"background" => ScriptMode.Background,
			_ => FailMode(value),
		};
	}

	private ScriptMode FailMode(string value)
	{
		Fail($"Invalid -ScriptMode value '{value}'. Valid values: Terminal, Standalone, Background.");
		return Mode;
	}

	private void Fail(string message) => Error ??= message;
}
