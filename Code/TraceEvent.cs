using System.Collections.Immutable;

namespace PerformanceTracing;

/// <summary>
/// Contains all information in regards to an event that occurred during tracing.
/// </summary>
public struct TraceEvent
{
	/// <summary>
	/// The name of the trace.
	/// </summary>
	public string Name { get; internal init; } = "Unknown";
	/// <summary>
	/// The categories that this trace fits into.
	/// </summary>
	public ImmutableArray<string> Categories { get; internal init; } = Tracing.DefaultCategories;
	/// <summary>
	/// The type of trace that this is.
	/// </summary>
	public TraceType Type { get; internal init; }
	/// <summary>
	/// The <see cref="System.Threading.Thread.ManagedThreadId"/> that this event came from.
	/// </summary>
	public int ThreadId { get; internal init; } = 1;

	/// <summary>
	/// The location in code to which this event came from.
	/// </summary>
	public SourceLocation Location { get; internal set; } = SourceLocation.None;
	/// <summary>
	/// The stack trace that leads up to this event.
	/// </summary>
	public string? StackTrace { get; internal set; }

	/// <summary>
	/// The nanosecond timestamp relative to the <see cref="Tracing.StartTimeTicks"/>.
	/// </summary>
	public double Timestamp { get; internal set; }
	/// <summary>
	/// The nanosecond duration relative to <see cref="Timestamp"/>.
	/// </summary>
	/// <remarks>
	/// This will only be set if <see cref="Type"/> is <see cref="TraceType.Performance"/>.
	/// </remarks>
	public double? Duration { get; internal set; }
	/// <summary>
	/// An additional value attached to this event.
	/// </summary>
	/// <remarks>
	/// This will be a <see cref="string"/> if <see cref="Type"/> is <see cref="TraceType.Meta"/>.
	/// <br/>
	/// This will be a <see cref="double"/> if <see cref="Type"/> is <see cref="TraceType.Counter"/>.
	/// <br/>
	/// Otherwise, this will be null.
	/// </remarks>
	public object? Value { get; internal set; }
	/// <summary>
	/// A shorthand getter to <see cref="Value"/> as a <see cref="double"/>.
	/// </summary>
	public readonly double ValueNumber => (double)Value!;
	/// <summary>
	/// A shorthand getter to <see cref="Value"/> as a <see cref="string"/>.
	/// </summary>
	public readonly string ValueString => (string)Value!;

	/// <summary>
	/// Initializes a default instance of <see cref="TraceEvent"/>.
	/// </summary>
	public TraceEvent()
	{
	}
}
