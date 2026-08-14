namespace TimestampCopy.Core;

/// <summary>
/// The cross-cutting switches an action needs: how loud to be, whether to ask before writing,
/// and whether there is a console to ask on.
/// </summary>
public readonly record struct Options(bool Quiet, bool SkipConfirm, ScriptMode Mode)
{
	/// <summary>
	/// Background mode has no console to prompt on, so it applies without asking - same as
	/// the script.
	/// </summary>
	public bool AutoConfirm => SkipConfirm || Mode == ScriptMode.Background;
}
