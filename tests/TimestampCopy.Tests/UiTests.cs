namespace TimestampCopy.Tests;

/// <summary>
/// Console behaviour, captured through Console.SetOut / SetIn. Colour is not asserted - Ui writes
/// it through Console.ForegroundColor, and a seam for capturing that costs more than it is worth.
/// Text and structure are what a paste actually shows the user.
/// <para>
/// Everything here mutates process-wide console state, so it lives in one class: xUnit runs the
/// tests inside a class sequentially.
/// </para>
/// </summary>
public sealed class UiTests : IDisposable
{
	private readonly TextWriter _out = Console.Out;
	private readonly TextReader _in = Console.In;

	public void Dispose()
	{
		Console.SetOut(_out);
		Console.SetIn(_in);
	}

	private static StringWriter Capture()
	{
		StringWriter writer = new();
		Console.SetOut(writer);
		return writer;
	}

	private static string[] Lines(StringWriter writer) =>
		writer.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

	// --- Confirm --------------------------------------------------------------------------

	[Theory]
	[InlineData("y")]
	[InlineData("Y")]
	public void Only_a_bare_y_applies(string answer)
	{
		Capture();
		Console.SetIn(new StringReader(answer));

		Assert.True(Ui.Confirm());
	}

	[Theory]
	[InlineData("n")]
	[InlineData("N")]
	[InlineData("yes")]     // the script takes "y" and nothing else
	[InlineData("Y ")]      // trailing space is not trimmed
	[InlineData(" y")]
	[InlineData("")]
	public void Anything_else_cancels(string answer)
	{
		Capture();
		Console.SetIn(new StringReader(answer));

		Assert.False(Ui.Confirm());
	}

	[Fact]
	public void End_of_input_cancels()
	{
		// A null ReadLine must not be treated as consent.
		Capture();
		Console.SetIn(new StringReader(string.Empty));

		Assert.False(Ui.Confirm());
	}

	[Fact]
	public void A_utf8_bom_in_front_of_the_answer_is_stripped()
	{
		// Windows PowerShell 5.1 prefixes one when piping into a native command, which would
		// otherwise turn "y" | tscp.exe -p ... into a silent cancel.
		Capture();
		Console.SetIn(new StringReader("﻿y"));

		Assert.True(Ui.Confirm());
	}

	[Fact]
	public void Confirm_prompts_with_the_scripts_wording()
	{
		StringWriter writer = Capture();
		Console.SetIn(new StringReader("n"));

		Ui.Confirm();

		Assert.Equal("Apply changes? (y/N): ", writer.ToString());
	}

	// --- HighlightDiff --------------------------------------------------------------------

	[Fact]
	public void The_diff_shows_the_old_value_then_the_new_one()
	{
		StringWriter writer = Capture();

		Ui.HighlightDiff("Date Created: ", "2024-01-02 03:04:05", "2024-06-07 08:09:10");

		Assert.Equal(
		[
			"Date Created:  2024-01-02 03:04:05 (old)",
			"Date Created:  2024-06-07 08:09:10 (new)",
		], Lines(writer));
	}

	[Fact]
	public void The_new_value_is_reassembled_with_the_original_separators()
	{
		// HighlightDiff splits on -, space and : to colour components individually, then rebuilds
		// the string from a fixed joiner table. If those disagree the timestamp comes out mangled.
		StringWriter writer = Capture();

		Ui.HighlightDiff("Date Modified:", "1999-12-31 23:59:59", "2024-06-07 08:09:10");

		Assert.EndsWith("2024-06-07 08:09:10 (new)", Lines(writer)[1]);
	}

	[Fact]
	public void An_unchanged_value_still_prints_both_lines()
	{
		StringWriter writer = Capture();

		Ui.HighlightDiff("Date Created: ", "2024-01-02 03:04:05", "2024-01-02 03:04:05");

		Assert.Equal(
		[
			"Date Created:  2024-01-02 03:04:05 (old)",
			"Date Created:  2024-01-02 03:04:05 (new)",
		], Lines(writer));
	}

	[Fact]
	public void The_label_is_written_verbatim_on_both_lines()
	{
		StringWriter writer = Capture();

		Ui.HighlightDiff("Date Modified:", "2024-01-02 03:04:05", "2024-06-07 08:09:10");

		Assert.All(Lines(writer), line => Assert.StartsWith("Date Modified: ", line));
	}

	// --- Write ----------------------------------------------------------------------------

	[Fact]
	public void Colour_does_not_change_what_is_written()
	{
		StringWriter writer = Capture();

		Ui.WriteLine("Done", ConsoleColor.Green);
		Ui.WriteLine("Error", ConsoleColor.Red);
		Ui.WriteLine("plain");

		Assert.Equal(["Done", "Error", "plain"], Lines(writer));
	}

	[Fact]
	public void WriteLine_with_no_argument_emits_a_blank_line()
	{
		StringWriter writer = Capture();

		Ui.WriteLine();

		Assert.Equal(Environment.NewLine, writer.ToString());
	}

	// --- Pause ----------------------------------------------------------------------------

	[Theory]
	[InlineData(ScriptMode.Terminal)]
	[InlineData(ScriptMode.Background)]
	public void Only_Standalone_pauses(ScriptMode mode)
	{
		// A Terminal or Background run must return immediately: ReadKey would block a scripted
		// run forever, and there is no window to keep open.
		StringWriter writer = Capture();

		Ui.Pause(mode);

		Assert.Equal(string.Empty, writer.ToString());
	}
}
