using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PerformanceTracing.Providers.Json;

internal sealed class TraceObject
{
	[JsonPropertyName( "traceEvents" )]
	public ConcurrentBag<TraceEvent> TraceEvents { get; } = new();
	[JsonPropertyName( "otherData" )]
	public Dictionary<string, object?> MetaData { get; } = new();
}
