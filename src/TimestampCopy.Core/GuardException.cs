namespace TimestampCopy.Core;

/// <summary>
/// A user-facing error that ends the run with exit code 1.
/// Equivalent to <c>Show-Guard-Message</c> in TimestampCopy.ps1: the host decides how to
/// present it (red console text in Terminal/Standalone mode, a message box in Background mode).
/// </summary>
public sealed class GuardException(string message) : Exception(message);
