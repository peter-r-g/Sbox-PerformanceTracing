using System.Collections.Immutable;

namespace PerformanceTracing;

public struct TraceEvent
{
	public string Name { get; internal init; } = "Unknown";
	public ImmutableArray<string> Categories { get; internal init; } = ImmutableArray.Create( "Uncategorized" );
	public TraceType Type { get; internal init; }
	public int ThreadId { get; internal init; } = 1;

	public SourceLocation Location { get; internal set; } = SourceLocation.None;
	public string StackTrace { get; internal set; } = string.Empty;

	public double Timestamp { get; internal set; }
	public double? Duration { get; internal set; }

	public object? Value { get; internal set; }
	public readonly double ValueNumber => (double)Value!;
	public readonly string ValueString => (string)Value!;

	public TraceEvent()
	{
	}
}
