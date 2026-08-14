using System.Runtime.InteropServices;

namespace TimestampCopy.Core;

/// <summary>
/// The Background-mode replacement for <c>[System.Windows.MessageBox]::Show</c>. Called through
/// user32 directly: WPF is neither trim- nor AOT-compatible, and pulling it in would cost more
/// than the whole rest of the program.
/// </summary>
public static partial class MessageBox
{
	private const uint MbOk = 0x00000000;
	private const uint MbIconExclamation = 0x00000030;

	// The script relies on WPF to bring its dialog forward. A plain MessageBoxW from a process
	// Explorer just launched can end up behind the active window, so ask for it explicitly.
	private const uint MbSetForeground = 0x00010000;

	public static void Show(string message) =>
		MessageBoxW(IntPtr.Zero, message, "Timestamp Copy", MbOk | MbIconExclamation | MbSetForeground);

	[LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
	private static partial int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
