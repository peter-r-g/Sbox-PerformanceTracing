using System;
using System.IO;

namespace PerformanceTracing;

/// <summary>
/// Provides a layer to store and serialize traces.
/// </summary>
public abstract class TraceStorageProvider
{
	/// <summary>
	/// Initializes a new trace.
	/// </summary>
	public abstract void Start();
	/// <summary>
	/// Cleans up any existing trace.
	/// </summary>
	public abstract void Stop();

	/// <summary>
	/// Adds some metadata to the current trace.
	/// </summary>
	/// <typeparam name="T">The type of the value being added.</typeparam>
	/// <param name="key">The key of the metadata.</param>
	/// <param name="value">The value of the entry.</param>
	public abstract void AddMetaData<T>( string key, T? value );
	/// <summary>
	/// Adds an event to the current trace.
	/// </summary>
	/// <param name="traceEvent">The event to add.</param>
	public abstract void AddEvent( in TraceEvent traceEvent );

	/// <summary>
	/// Writes the providers data to a stream.
	/// </summary>
	/// <param name="stream">The stream to write to.</param>
	public virtual void WriteToStream( Stream stream )
	{
		throw new NotSupportedException( nameof( WriteToStream ) );
	}
}
