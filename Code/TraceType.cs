namespace PerformanceTracing;

/// <summary>
/// Defines every type of trace that is available.
/// </summary>
public enum TraceType
{
	/// <summary>
	/// A counter for number values.
	/// </summary>
	Counter,
	/// <summary>
	/// Marks a point in time that something happened.
	/// </summary>
	Marker,
	/// <summary>
	/// An execution time log.
	/// </summary>
	Performance,
	/// <summary>
	/// A setter for metadata.
	/// </summary>
	Meta
}
