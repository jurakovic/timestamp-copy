namespace TimestampCopy.Tests;

/// <summary>
/// Pins the strings written to HKEY_CLASSES_ROOT. Nothing here touches the registry - only
/// <see cref="ContextMenu.Command"/> and the tables it is driven from, which is the part that
/// fails silently: a misplaced quote produces a menu item that does nothing when clicked, with
/// no error anywhere.
/// <para>
/// The expected values are built from <see cref="Constants.ExePath"/> rather than hardcoded,
/// because the installer deliberately registers wherever the folder happens to sit.
/// </para>
/// </summary>
public class ContextMenuTests
{
	private static readonly string Exe = Constants.ExePath;
	private static readonly string ExeW = Constants.ExePathBackground;

	[Fact]
	public void Standalone_commands_are_exactly_what_the_script_wrote()
	{
		Assert.Equal($"\"{Exe}\" -m Standalone -c \"%1\"", Command(ScriptMode.Standalone, "-c", true));
		Assert.Equal($"\"{Exe}\" -m Standalone -p \"%1\"", Command(ScriptMode.Standalone, "-p", true));
		Assert.Equal($"\"{Exe}\" -m Standalone -pc \"%1\"", Command(ScriptMode.Standalone, "-pc", true));
		Assert.Equal($"\"{Exe}\" -m Standalone -pm \"%1\"", Command(ScriptMode.Standalone, "-pm", true));
		Assert.Equal($"\"{Exe}\" -m Standalone -z", Command(ScriptMode.Standalone, "-z", false));
	}

	[Fact]
	public void Terminal_commands_differ_from_Standalone_only_in_the_mode()
	{
		Assert.Equal($"\"{Exe}\" -m Terminal -c \"%1\"", Command(ScriptMode.Terminal, "-c", true));
		Assert.Equal($"\"{Exe}\" -m Terminal -z", Command(ScriptMode.Terminal, "-z", false));
	}

	[Fact]
	public void Background_runs_tscpw_with_no_mode_and_no_conhost_prefix()
	{
		// A windowed exe needs neither the headless conhost prefix the script used nor -ScriptMode.
		Assert.Equal($"\"{ExeW}\" -c \"%1\"", Command(ScriptMode.Background, "-c", true));
		Assert.Equal($"\"{ExeW}\" -z", Command(ScriptMode.Background, "-z", false));

		string command = Command(ScriptMode.Background, "-p", true);
		Assert.DoesNotContain("conhost", command);
		Assert.DoesNotContain(" -m ", command);
		Assert.DoesNotContain(Exe, command);
	}

	[Fact]
	public void Only_Undo_omits_the_path_placeholder()
	{
		foreach ((string _, string _, string flag, bool path) in ContextMenu.Items)
		{
			string command = Command(ScriptMode.Standalone, flag, path);

			if (flag == "-z")
				Assert.DoesNotContain("%1", command);
			else
				Assert.EndsWith(" \"%1\"", command);
		}
	}

	[Fact]
	public void The_path_placeholder_is_double_quoted_not_single_quoted()
	{
		// The script passed '%1' through PowerShell; the exe takes "%1". Getting this wrong breaks
		// every path containing a space, and only those.
		string command = Command(ScriptMode.Standalone, "-c", true);

		Assert.Contains("\"%1\"", command);
		Assert.DoesNotContain("'%1'", command);
	}

	[Fact]
	public void The_executable_path_is_quoted_so_it_survives_Program_Files()
	{
		Assert.StartsWith($"\"{Exe}\"", Command(ScriptMode.Standalone, "-c", true));
		Assert.StartsWith($"\"{ExeW}\"", Command(ScriptMode.Background, "-c", true));
	}

	[Fact]
	public void The_five_menu_items_keep_their_order_labels_and_flags()
	{
		// The numeric prefixes are what order the submenu; the labels are user-visible, and the
		// literal quotes inside two of them are part of the string.
		Assert.Equal(
		[
			(@"shell\010-Copy", "Copy", "-c", true),
			(@"shell\020-Paste", "Paste", "-p", true),
			(@"shell\030-PasteDateCreated", "Paste \"Date Created\"", "-pc", true),
			(@"shell\040-PasteDateModified", "Paste \"Date Modified\"", "-pm", true),
			(@"shell\050-Undo", "Undo", "-z", false),
		], ContextMenu.Items);
	}

	[Fact]
	public void All_three_root_keys_are_registered()
	{
		// Files, folders and shortcuts. Dropping lnkfile is how .lnk support disappears quietly.
		Assert.Equal(
		[
			@"*\shell\TimestampCopy",
			@"Directory\shell\TimestampCopy",
			@"lnkfile\shell\TimestampCopy",
		], ContextMenu.RootKeys);
	}

	[Fact]
	public void Every_item_in_every_mode_produces_a_usable_command()
	{
		foreach (ScriptMode mode in Enum.GetValues<ScriptMode>())
		{
			foreach ((string _, string _, string flag, bool path) in ContextMenu.Items)
			{
				string command = Command(mode, flag, path);

				Assert.DoesNotContain("  ", command);
				Assert.Equal(command.Trim(), command);
				Assert.Contains($" {flag}", command);
			}
		}
	}

	private static string Command(ScriptMode mode, string flag, bool path) =>
		ContextMenu.Command(mode, flag, path);
}
