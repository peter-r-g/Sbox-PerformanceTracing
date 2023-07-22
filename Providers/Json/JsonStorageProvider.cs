using System;
using System.IO;
using System.Text.Json;

namespace PerformanceTracing.Providers;

/// <summary>
/// A provider to save traces in chromes trace event JSON format.
/// </summary>
public sealed class JsonStorageProvider : TraceStorageProvider
{
	private static TraceObject? CurrentTraceObject { get; set; }

	/// <inheritdoc/>
	public override void Start()
	{
		CurrentTraceObject = new TraceObject()
		{
			TraceEvents =
			{
				new TraceEvent
				{
					Name = "process_name",
					EventType = "M",
					ProcessId = 1,
					Arguments =
					{
						{ "name", "S&box" }
					}
				},
				new TraceEvent
				{
					Name = "thread_name",
					EventType = "M",
					ThreadId = 1,
					Arguments =
					{
						{ "name", "MainThread" }
					}
				},
				new TraceEvent
				{
					Name = "thread_anem",
					EventType = "M",
					ThreadId = 2,
					Arguments =
					{
						{ "name", "UnknownThread" }
					}
				}
			}
		};
	}

	/// <inheritdoc/>
	public override void Stop()
	{
		CurrentTraceObject = null;
	}

	/// <inheritdoc/>
	public override void AddMetaData<T>( string key, T? value ) where T : default
	{
		if ( CurrentTraceObject!.MetaData.ContainsKey( key ) )
			throw new ArgumentException( $"A metadata entry with the key \"{key}\" already exists" );

		CurrentTraceObject.MetaData.Add(key, value);
	}

	/// <inheritdoc/>
	public override void AddEvent( in TraceEvent traceEvent )
	{
		CurrentTraceObject!.TraceEvents.Add( traceEvent );
	}

	/// <inheritdoc/>
	public override void WriteToStream( Stream stream )
	{
		if ( !stream.CanWrite )
			throw new ArgumentException( "The stream cannot be written to", nameof( stream ) );

		JsonSerializer.Serialize( stream, CurrentTraceObject );
	}
}
