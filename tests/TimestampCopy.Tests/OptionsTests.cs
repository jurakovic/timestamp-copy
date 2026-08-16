namespace TimestampCopy.Tests;

public class OptionsTests
{
	[Theory]
	[InlineData(false, ScriptMode.Terminal, false)]
	[InlineData(false, ScriptMode.Standalone, false)]
	[InlineData(true, ScriptMode.Terminal, true)]
	[InlineData(true, ScriptMode.Standalone, true)]
	public void Confirmation_is_skipped_only_when_asked_for(bool skipConfirm, ScriptMode mode,
		bool expected) =>
		Assert.Equal(expected, new Options(Quiet: false, skipConfirm, mode).AutoConfirm);

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Background_always_auto_confirms(bool skipConfirm)
	{
		// There is no console to prompt on, so tscpw applies without asking - same as the script.
		Assert.True(new Options(Quiet: true, skipConfirm, ScriptMode.Background).AutoConfirm);
	}

	[Fact]
	public void Quiet_does_not_imply_auto_confirm()
	{
		// -q silences the output but the prompt still appears, matching the script.
		Assert.False(new Options(Quiet: true, SkipConfirm: false, ScriptMode.Terminal).AutoConfirm);
	}
}
