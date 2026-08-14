using TimestampCopy.Core;

namespace TimestampCopy.Cli;

/// <summary>
/// Windowed entry point: Background mode. There is no console, so nothing is printed and guard
/// messages are shown as message boxes.
/// </summary>
internal static class Program
{
	private static int Main(string[] argv) => Host.Run(argv, background: true);
}
