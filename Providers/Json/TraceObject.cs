using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace PerformanceTracing.Providers.Json;

internal sealed class TraceObject
{
	[JsonPropertyName( "traceEvents" )]
	public ConcurrentBag<TraceEvent> TraceEvents { get; } = new();
	[JsonPropertyName( "otherData" )]
	public ConcurrentDictionary<string, object?> MetaData { get; } = new();

	internal TraceObject()
	{
	}

	internal TraceObject( TraceObject other )
	{
		TraceEvents = new ConcurrentBag<TraceEvent>( other.TraceEvents.ToArray() );
		MetaData = new ConcurrentDictionary<string, object?>( other.MetaData.ToArray() );
	}
}
