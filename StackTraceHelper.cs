using System;
using System.Collections.Concurrent;
using System.Linq;

namespace PerformanceTracing;

internal static class StackTraceHelper
{
	private static ConcurrentDictionary<string, string[]> StackTraceEntries { get; } = new();

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

	internal static string GetStackTrace( int entry = 0 )
	{
		entry += 2;
		return string.Join( '\n', Environment.StackTrace.Split( '\n' ).Skip( entry ).Select( str => str.Trim() ) );
	}
}
