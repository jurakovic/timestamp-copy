using System.Text;

namespace TimestampCopy.Tests;

/// <summary>
/// The on-disk clipboard format, which is the compatibility contract with the 2.x PowerShell
/// releases: base64(utf8(...)), LF between fields, trailing newline from Set-Content. One wrong
/// byte here and an upgrade silently loses whatever was already copied.
/// </summary>
public sealed class ClipTests : IDisposable
{
	private readonly string _dir = Directory.CreateTempSubdirectory("tscp-tests-").FullName;

	private string Path(string name) => System.IO.Path.Combine(_dir, name);

	public void Dispose() => Directory.Delete(_dir, recursive: true);

	// --- format ---------------------------------------------------------------------------

	[Fact]
	public void Write_produces_base64_followed_by_a_trailing_newline()
	{
		string path = Path("clip");

		Clip.Write(path, "2024-01-02 03:04:05\n2024-06-07 08:09:10");

		string expected = Convert.ToBase64String(
			Encoding.UTF8.GetBytes("2024-01-02 03:04:05\n2024-06-07 08:09:10"));

		Assert.Equal(expected + Environment.NewLine, File.ReadAllText(path));
	}

	[Fact]
	public void Write_emits_no_byte_order_mark()
	{
		// The payload is base64, i.e. pure ASCII. A BOM would break the PowerShell reader.
		string path = Path("clip");

		Clip.Write(path, "a\nb");

		byte[] bytes = File.ReadAllBytes(path);
		Assert.NotEqual<byte>(0xEF, bytes[0]);
		Assert.All(bytes, b => Assert.True(b < 0x80));
	}

	[Fact]
	public void Write_creates_the_containing_directory()
	{
		string path = System.IO.Path.Combine(_dir, "nested", "deeper", "clip");

		Clip.Write(path, "a\nb");

		Assert.True(File.Exists(path));
	}

	[Fact]
	public void Read_round_trips_what_Write_wrote()
	{
		string path = Path("clip");

		Clip.Write(path, "one\ntwo\nthree");

		Assert.Equal(["one", "two", "three"], Clip.Read(path));
	}

	[Fact]
	public void Read_tolerates_a_missing_trailing_newline()
	{
		// PowerShell's Set-Content appends one; a hand-written file may not have it.
		string path = Path("clip");
		File.WriteAllText(path, Convert.ToBase64String(Encoding.UTF8.GetBytes("a\nb")));

		Assert.Equal(["a", "b"], Clip.Read(path));
	}

	[Fact]
	public void Read_tolerates_a_CRLF_written_by_PowerShell()
	{
		string path = Path("clip");
		File.WriteAllText(path, Convert.ToBase64String(Encoding.UTF8.GetBytes("a\nb")) + "\r\n");

		Assert.Equal(["a", "b"], Clip.Read(path));
	}

	[Fact]
	public void Fields_are_separated_by_LF_not_CRLF()
	{
		// The separator inside the payload is LF even though the file ends with the platform
		// newline. Writing CRLF inside would give the PowerShell reader a trailing \r on field 0.
		string path = Path("clip");

		Clip.WriteTimestampsTo(path, "2024-01-02 03:04:05", "2024-06-07 08:09:10");

		string decoded = Encoding.UTF8.GetString(
			Convert.FromBase64String(File.ReadAllText(path).Trim()));

		Assert.Equal("2024-01-02 03:04:05\n2024-06-07 08:09:10", decoded);
		Assert.DoesNotContain('\r', decoded);
	}

	[Fact]
	public void The_undo_payload_is_path_then_created_then_modified()
	{
		string path = Path("clip-undo");

		Clip.WriteUndoTo(path, @"D:\some file.txt", "2024-01-02 03:04:05", "2024-06-07 08:09:10");

		Assert.Equal([@"D:\some file.txt", "2024-01-02 03:04:05", "2024-06-07 08:09:10"],
			Clip.Read(path));
	}

	[Fact]
	public void A_path_with_non_ascii_characters_survives_the_round_trip()
	{
		string path = Path("clip-undo");
		const string awkward = @"D:\naziv sa razmakom i čćžšđ\Foo Bar čćž.txt";

		Clip.WriteUndoTo(path, awkward, "2024-01-02 03:04:05", "2024-06-07 08:09:10");

		Assert.Equal(awkward, Clip.Read(path)[0]);
	}

	// --- Guard-Clipboard ------------------------------------------------------------------

	private const string Empty = "Timestamps clipboard empty. Copy new timestamps.    ";
	private const string Corrupted = "Timestamps clipboard corrupted. Copy new timestamps.";

	[Fact]
	public void A_missing_clipboard_reports_empty_with_the_scripts_trailing_spaces()
	{
		GuardException ex = Assert.Throws<GuardException>(
			() => Clip.ReadTimestampsFrom(Path("does-not-exist")));

		Assert.Equal(Empty, ex.Message);
	}

	[Theory]
	[InlineData("not base64 at all!")]              // FormatException from the decoder
	[InlineData("")]                                 // decodes to nothing
	public void An_undecodable_clipboard_reports_corrupted(string content)
	{
		string path = Path("clip");
		File.WriteAllText(path, content);

		Assert.Equal(Corrupted,
			Assert.Throws<GuardException>(() => Clip.ReadTimestampsFrom(path)).Message);
	}

	[Theory]
	[InlineData("2024-01-02 03:04:05")]                                          // one field
	[InlineData("2024-01-02 03:04:05\n2024-06-07 08:09:10\n2024-06-07 08:09:10")] // three fields
	public void A_clipboard_with_the_wrong_field_count_reports_corrupted(string payload)
	{
		Assert.Equal(Corrupted,
			Assert.Throws<GuardException>(() => Clip.ReadTimestampsFrom(WriteRaw("clip", payload))).Message);
	}

	[Theory]
	[InlineData("not a date\n2024-06-07 08:09:10")]
	[InlineData("2024-01-02 03:04:05\nnot a date")]
	[InlineData("2024-1-2 3:4:5\n2024-06-07 08:09:10")]   // right shape, wrong padding
	[InlineData("02/01/2024 03:04:05\n2024-06-07 08:09:10")]
	public void A_clipboard_with_an_unparsable_timestamp_reports_corrupted(string payload)
	{
		Assert.Equal(Corrupted,
			Assert.Throws<GuardException>(() => Clip.ReadTimestampsFrom(WriteRaw("clip", payload))).Message);
	}

	[Fact]
	public void A_well_formed_clipboard_reads_back_both_timestamps()
	{
		string path = WriteRaw("clip", "2024-01-02 03:04:05\n2024-06-07 08:09:10");

		Assert.Equal(["2024-01-02 03:04:05", "2024-06-07 08:09:10"], Clip.ReadTimestampsFrom(path));
	}

	// --- Guard-Undo-Clipboard -------------------------------------------------------------

	private const string UndoEmpty = "Timestamps undo clipboard empty. Paste some timestamps.   ";
	private const string UndoCorrupted = "Timestamps undo clipboard corrupted. Paste new timestamps.";

	[Fact]
	public void A_missing_undo_clipboard_reports_empty_with_the_scripts_trailing_spaces()
	{
		GuardException ex = Assert.Throws<GuardException>(
			() => Clip.ReadUndoFrom(Path("does-not-exist")));

		Assert.Equal(UndoEmpty, ex.Message);
	}

	[Fact]
	public void An_undecodable_undo_clipboard_reports_corrupted()
	{
		string path = Path("clip-undo");
		File.WriteAllText(path, "not base64 at all!");

		Assert.Equal(UndoCorrupted,
			Assert.Throws<GuardException>(() => Clip.ReadUndoFrom(path)).Message);
	}

	[Theory]
	[InlineData(@"D:\f.txt" + "\n2024-01-02 03:04:05")]                            // two fields
	[InlineData(@"D:\f.txt" + "\n2024-01-02 03:04:05\n2024-06-07 08:09:10\nextra")] // four
	public void An_undo_clipboard_with_the_wrong_field_count_reports_corrupted(string payload)
	{
		Assert.Equal(UndoCorrupted,
			Assert.Throws<GuardException>(() => Clip.ReadUndoFrom(WriteRaw("clip-undo", payload))).Message);
	}

	[Fact]
	public void An_undo_clipboard_with_an_unparsable_timestamp_reports_corrupted()
	{
		string path = WriteRaw("clip-undo", @"D:\f.txt" + "\nnot a date\n2024-06-07 08:09:10");

		Assert.Equal(UndoCorrupted,
			Assert.Throws<GuardException>(() => Clip.ReadUndoFrom(path)).Message);
	}

	[Fact]
	public void Field_zero_of_the_undo_clipboard_is_a_path_and_is_not_parsed_as_a_timestamp()
	{
		// The whole point of firstTimestamp: 1. A path is never a valid date, so parsing it
		// would make every undo clipboard "corrupted".
		string path = WriteRaw("clip-undo",
			@"D:\definitely not a date.txt" + "\n2024-01-02 03:04:05\n2024-06-07 08:09:10");

		Assert.Equal(@"D:\definitely not a date.txt", Clip.ReadUndoFrom(path)[0]);
	}

	private string WriteRaw(string name, string payload)
	{
		string path = Path(name);
		Clip.Write(path, payload);
		return path;
	}
}
