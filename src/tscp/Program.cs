using TimestampCopy.Core;

namespace TimestampCopy.Cli;

/// <summary>
/// Console entry point: Terminal and Standalone modes.
/// Background mode gets its own WinExe (tscpw) so there is no console to flash.
/// </summary>
internal static class Program
{
	private static int Main(string[] argv) => Host.Run(argv, background: false);
}
