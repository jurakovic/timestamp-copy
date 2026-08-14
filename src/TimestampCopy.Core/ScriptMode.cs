namespace TimestampCopy.Core;

/// <summary>
/// Mirrors the <c>-ScriptMode</c> parameter of TimestampCopy.ps1.
/// </summary>
public enum ScriptMode
{
	/// <summary>Run from an existing terminal; no "press any key" pause.</summary>
	Terminal,

	/// <summary>Launched from Explorer into its own window; pause before it closes.</summary>
	Standalone,

	/// <summary>No console at all; guard messages go to a message box (see tscpw).</summary>
	Background,
}
