namespace TimestampCopy.Tests;

/// <summary>
/// The command line is the contract the registry entries are written against, and it has to keep
/// accepting exactly what TimestampCopy.ps1 accepted. Args.Parse is pure, so all of this is
/// straight assertion with no I/O.
/// </summary>
public class ArgsTests
{
	private const string Path = @"D:\some file.txt";

	[Theory]
	[InlineData("-c")]
	[InlineData("-C")]
	[InlineData("-copy")]
	[InlineData("-Copy")]
	[InlineData("-COPY")]
	[InlineData("/c")]
	[InlineData("/Copy")]
	public void Copy_is_accepted_in_every_spelling(string flag)
	{
		Args args = Args.Parse([flag, Path]);

		Assert.Null(args.Error);
		Assert.Equal(Path, args.Copy);
	}

	[Theory]
	[InlineData("-p", "-Paste")]
	[InlineData("-pc", "-PasteDateCreated")]
	[InlineData("-pm", "-PasteDateModified")]
	public void Paste_short_and_long_forms_agree(string @short, string @long)
	{
		Args fromShort = Args.Parse([@short, Path]);
		Args fromLong = Args.Parse([@long, Path]);

		Assert.Null(fromShort.Error);
		Assert.Null(fromLong.Error);
		Assert.Equal(Value(fromShort), Value(fromLong));
		Assert.Equal(Path, Value(fromShort));

		static string? Value(Args a) => a.Paste ?? a.PasteDateCreated ?? a.PasteDateModified;
	}

	[Theory]
	[InlineData("-h")]
	[InlineData("-help")]
	[InlineData("-?")]
	public void Help_is_accepted_in_every_spelling(string flag) =>
		Assert.True(Args.Parse([flag]).Help);

	[Theory]
	[InlineData("-v", "-version")]
	[InlineData("-i", "-install")]
	[InlineData("-b", "-installbackgroundmode")]
	[InlineData("-u", "-uninstall")]
	[InlineData("-z", "-undo")]
	[InlineData("-q", "-quiet")]
	[InlineData("-y", "-skipconfirm")]
	public void Switches_are_accepted_in_short_and_long_form(string @short, string @long)
	{
		Assert.Null(Args.Parse([@short]).Error);
		Assert.Null(Args.Parse([@long]).Error);
	}

	[Fact]
	public void Each_switch_sets_only_its_own_flag()
	{
		Assert.True(Args.Parse(["-v"]).Version);
		Assert.True(Args.Parse(["-i"]).Install);
		Assert.True(Args.Parse(["-b"]).InstallBackgroundMode);
		Assert.True(Args.Parse(["-u"]).Uninstall);
		Assert.True(Args.Parse(["-z"]).Undo);
		Assert.True(Args.Parse(["-q"]).Quiet);
		Assert.True(Args.Parse(["-y"]).SkipConfirm);
	}

	[Fact]
	public void Nothing_is_set_when_nothing_is_passed()
	{
		Args args = Args.Parse([]);

		Assert.Null(args.Error);
		Assert.False(args.Help || args.Version || args.Install || args.InstallBackgroundMode
			|| args.Uninstall || args.Undo || args.Quiet || args.SkipConfirm);
		Assert.Null(args.Copy);
		Assert.Null(args.Paste);
	}

	[Theory]
	[InlineData("Terminal", ScriptMode.Terminal)]
	[InlineData("standalone", ScriptMode.Standalone)]
	[InlineData("BACKGROUND", ScriptMode.Background)]
	public void ScriptMode_is_parsed_case_insensitively(string value, ScriptMode expected)
	{
		Assert.Equal(expected, Args.Parse(["-m", value]).Mode);
		Assert.Equal(expected, Args.Parse(["-ScriptMode", value]).Mode);
	}

	[Fact]
	public void Mode_defaults_to_Terminal_and_the_default_can_be_overridden()
	{
		Assert.Equal(ScriptMode.Terminal, Args.Parse(["-c", Path]).Mode);
		Assert.Equal(ScriptMode.Background, Args.Parse(["-c", Path], ScriptMode.Background).Mode);
	}

	[Fact]
	public void Flags_combine()
	{
		Args args = Args.Parse(["-m", "Standalone", "-q", "-y", "-p", Path]);

		Assert.Null(args.Error);
		Assert.Equal(ScriptMode.Standalone, args.Mode);
		Assert.True(args.Quiet);
		Assert.True(args.SkipConfirm);
		Assert.Equal(Path, args.Paste);
	}

	[Fact]
	public void Options_carries_quiet_skipconfirm_and_mode()
	{
		Options options = Args.Parse(["-q", "-y", "-m", "Standalone"]).Options;

		Assert.Equal(new Options(Quiet: true, SkipConfirm: true, ScriptMode.Standalone), options);
	}

	// --- guards ---------------------------------------------------------------------------

	[Fact]
	public void Unknown_parameter_is_reported_with_the_argument_as_written() =>
		Assert.Equal("Unknown parameter '-nope'.", Args.Parse(["-nope"]).Error);

	[Fact]
	public void A_bare_word_is_reported_as_unexpected() =>
		Assert.Equal("Unexpected argument 'copy'.", Args.Parse(["copy"]).Error);

	[Fact]
	public void A_lone_dash_is_too_short_to_be_a_flag() =>
		Assert.Equal("Unexpected argument '-'.", Args.Parse(["-"]).Error);

	[Fact]
	public void A_parameter_at_the_end_of_the_line_is_missing_its_value() =>
		Assert.Equal("Parameter '-c' requires a value.", Args.Parse(["-c"]).Error);

	[Fact]
	public void Two_actions_cannot_be_combined() =>
		Assert.Equal(
			"Parameters -Copy, -Paste, -PasteDateCreated, -PasteDateModified, -Undo cannot be used together.",
			Args.Parse(["-c", Path, "-p", Path]).Error);

	[Fact]
	public void Undo_counts_as_an_action_for_that_guard() =>
		Assert.NotNull(Args.Parse(["-c", Path, "-z"]).Error);

	[Fact]
	public void Two_install_parameters_cannot_be_combined() =>
		Assert.Equal(
			"Parameters -Install, -InstallBackgroundMode, -Uninstall cannot be used together.",
			Args.Parse(["-i", "-u"]).Error);

	[Fact]
	public void Invalid_ScriptMode_lists_the_valid_values() =>
		Assert.Equal(
			"Invalid -ScriptMode value 'Silent'. Valid values: Terminal, Standalone, Background.",
			Args.Parse(["-m", "Silent"]).Error);

	[Fact]
	public void The_first_error_wins_and_parsing_stops()
	{
		// -alsobad is never reached: ParseCore returns as soon as Error is set.
		Assert.Equal("Unknown parameter '-bad'.", Args.Parse(["-bad", "-alsobad"]).Error);
	}

	[Fact]
	public void Quiet_parsed_before_the_error_still_applies()
	{
		// The host reports args.Error through the Options parsed so far, so -q has to survive.
		Args args = Args.Parse(["-q", "-bad"]);

		Assert.NotNull(args.Error);
		Assert.True(args.Quiet);
	}

	[Fact]
	public void A_value_that_looks_like_a_flag_is_still_taken_as_the_value()
	{
		// Documenting existing behaviour rather than endorsing it: -c consumes the next argument
		// unconditionally, exactly as the PowerShell parameter binder did.
		Assert.Equal("-p", Args.Parse(["-c", "-p"]).Copy);
	}

	[Fact]
	public void Paths_with_spaces_and_non_ascii_characters_survive_intact()
	{
		const string awkward = @"D:\naziv sa razmakom i čćžšđ\Foo Bar čćž.txt";

		Assert.Equal(awkward, Args.Parse(["-c", awkward]).Copy);
	}
}
