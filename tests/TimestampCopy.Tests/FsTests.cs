namespace TimestampCopy.Tests;

/// <summary>
/// The timestamp format and the one-code-path-for-files-and-folders rule. The format string is
/// half of the compatibility contract with the 2.x releases; the other half is in
/// <see cref="ClipTests"/>.
/// </summary>
public sealed class FsTests : IDisposable
{
	private readonly string _dir = Directory.CreateTempSubdirectory("tscp-tests-").FullName;

	public void Dispose() => Directory.Delete(_dir, recursive: true);

	[Fact]
	public void Format_is_the_fixed_sortable_pattern_the_script_used() =>
		Assert.Equal("2024-01-02 03:04:05", Fs.Format(new DateTime(2024, 1, 2, 3, 4, 5)));

	[Fact]
	public void Format_pads_every_component_to_two_digits() =>
		Assert.Equal("0009-08-07 06:05:04", Fs.Format(new DateTime(9, 8, 7, 6, 5, 4)));

	[Fact]
	public void Format_drops_sub_second_precision()
	{
		// The clipboard has one-second resolution, so a paste is not bit-identical to the source.
		DateTime value = new DateTime(2024, 1, 2, 3, 4, 5).AddMilliseconds(999);

		Assert.Equal("2024-01-02 03:04:05", Fs.Format(value));
	}

	[Fact]
	public void Format_and_Parse_round_trip()
	{
		DateTime value = new(2024, 1, 2, 3, 4, 5);

		Assert.Equal(value, Fs.Parse(Fs.Format(value)));
	}

	[Theory]
	[InlineData("2024-01-02 03:04:05")]
	[InlineData("0001-01-01 00:00:00")]
	[InlineData("9999-12-31 23:59:59")]
	public void Well_formed_values_parse(string value) => Assert.True(Fs.TryParse(value, out _));

	[Theory]
	[InlineData("")]
	[InlineData("not a date")]
	[InlineData("2024-1-2 3:4:5")]            // unpadded
	[InlineData("2024-01-02T03:04:05")]       // ISO separator
	[InlineData("02/01/2024 03:04:05")]       // locale format
	[InlineData("2024-01-02 03:04")]          // no seconds
	[InlineData("2024-01-02 03:04:05.123")]   // sub-second
	[InlineData(" 2024-01-02 03:04:05")]      // leading space
	[InlineData("2024-13-02 03:04:05")]       // month 13
	public void Malformed_values_do_not_parse(string value) => Assert.False(Fs.TryParse(value, out _));

	[Fact]
	public void Parse_throws_on_a_malformed_value() =>
		Assert.Throws<FormatException>(() => Fs.Parse("not a date"));

	[Fact]
	public void A_folder_reads_as_DirectoryInfo_and_a_file_as_FileInfo()
	{
		string file = Path.Combine(_dir, "file.txt");
		File.WriteAllText(file, "x");

		Assert.IsType<DirectoryInfo>(Fs.GetInfo(_dir));
		Assert.IsType<FileInfo>(Fs.GetInfo(file));
	}

	[Fact]
	public void A_shortcut_takes_the_file_branch()
	{
		// A .lnk is a file, so it needs no special case - it is one of the three registered roots.
		string lnk = Path.Combine(_dir, "shortcut.lnk");
		File.WriteAllText(lnk, "x");

		Assert.IsType<FileInfo>(Fs.GetInfo(lnk));
	}

	[Fact]
	public void A_nonexistent_path_falls_back_to_FileInfo() =>
		Assert.IsType<FileInfo>(Fs.GetInfo(Path.Combine(_dir, "nope")));

	[Fact]
	public void Exists_covers_both_files_and_folders()
	{
		string file = Path.Combine(_dir, "file.txt");
		File.WriteAllText(file, "x");

		Assert.True(Fs.Exists(_dir));
		Assert.True(Fs.Exists(file));
		Assert.False(Fs.Exists(Path.Combine(_dir, "nope")));
	}

	[Fact]
	public void GuardPathExists_names_the_path_it_could_not_find()
	{
		string missing = Path.Combine(_dir, "nope");

		GuardException ex = Assert.Throws<GuardException>(() => Fs.GuardPathExists(missing));

		Assert.Equal($"Path '{missing}' does not exist.", ex.Message);
	}

	[Fact]
	public void GuardPathExists_passes_for_a_folder() => Fs.GuardPathExists(_dir);

	[Fact]
	public void Timestamps_written_through_FileSystemInfo_read_back_in_the_same_format()
	{
		// The whole point of the tool, in miniature: format out, parse in, set, format out again.
		string file = Path.Combine(_dir, "file.txt");
		File.WriteAllText(file, "x");

		FileSystemInfo item = Fs.GetInfo(file);
		item.CreationTime = Fs.Parse("2001-02-03 04:05:06");
		item.LastWriteTime = Fs.Parse("2002-03-04 05:06:07");

		FileSystemInfo reread = Fs.GetInfo(file);
		Assert.Equal("2001-02-03 04:05:06", Fs.Format(reread.CreationTime));
		Assert.Equal("2002-03-04 05:06:07", Fs.Format(reread.LastWriteTime));
	}
}
