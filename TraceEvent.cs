using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PerformanceTracing;

internal struct TraceEvent
{
	[JsonPropertyName( "name" )]
	public string Name { get; set; } = "Unknown";
	[JsonPropertyName( "cat" )]
	public string Categories { get; set; } = "None";
	[JsonPropertyName( "ph" )]
	public string EventType { get; set; } = string.Empty;
	[JsonPropertyName( "ts" )]
	public double Timestamp { get; set; }
	[JsonPropertyName( "dur" )]
	public double Duration { get; set; }
	[JsonPropertyName( "pid" )]
	public int ProcessId { get; set; } = 1;
	[JsonPropertyName( "tid" )]
	public int ThreadId { get; set; } = 1;
	[JsonPropertyName( "args" )]
	public Dictionary<string, string> Arguments { get; init; } = new();

	public TraceEvent()
	{
	}
}
