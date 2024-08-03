using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace PerformanceTracing;

internal static class StackTraceHelper
{
	private static ConcurrentDictionary<string, string[]> StackTraceEntries { get; } = new();

	[MethodImpl( MethodImplOptions.NoInlining )]
	internal static string GetTraceEntrySignature( int entry = 0 )
	{
		entry += 2;

		var stackTrace = Environment.StackTrace;
		if ( StackTraceEntries.TryGetValue( stackTrace, out var entries ) )
			return entries[entry];

		var traceEntries = stackTrace.Split( '\n', StringSplitOptions.TrimEntries );
		for ( var i = 0; i < traceEntries.Length; i++ )
		{
			ReadOnlySpan<char> traceEntry = traceEntries[i];
			var endIndex = traceEntry.IndexOf( " in" );
			if ( endIndex == -1 )
				endIndex = traceEntry.Length - 1;

			traceEntries[i] = traceEntry[3..endIndex].ToString();
		}

		StackTraceEntries.TryAdd( stackTrace, traceEntries );
		return traceEntries[entry];
	}

	[MethodImpl( MethodImplOptions.NoInlining )]
	internal static string GetStackTrace( int entry = 0 )
	{
		entry += 2;
		ReadOnlySpan<char> stackTrace = Environment.StackTrace;

		for ( var i = 0; i < entry; i++ )
		{
			var currentIndex = stackTrace.IndexOf( '\n' );
			stackTrace = stackTrace[(currentIndex + 1)..];
		}

		return stackTrace.ToString();
	}
}
