using System;

namespace PerformanceTracing.Providers.Json;

internal static class TraceTypeExtensions
{
	internal static string ToChromeTraceFormat( this TraceType type ) => type switch
	{
		TraceType.Counter => "C",
		TraceType.Marker => "R",
		TraceType.Performance => "X",
		TraceType.Meta => "M",
		_ => throw new ArgumentException( "Received invalid type", nameof( type ) )
	};
}
