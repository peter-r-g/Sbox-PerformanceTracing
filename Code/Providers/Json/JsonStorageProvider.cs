using PerformanceTracing.Util;
using Sandbox;
using System;
using System.IO;
using System.Text.Json;

namespace PerformanceTracing.Providers.Json;

/// <summary>
/// A provider to save traces in chromes trace event JSON format.
/// </summary>
public sealed class JsonStorageProvider : TraceStorageProvider
{
	private TraceObject? CurrentTraceObject { get; set; }
	private JsonSerializerOptions Options { get; }

	private ConcurrentHashSet<int>? NamedThreads { get; set; }

	/// <summary>
	/// Initializes a default instance of <see cref="JsonStorageProvider"/>.
	/// </summary>
	public JsonStorageProvider()
	{
		Options = new JsonSerializerOptions()
		{
			Converters =
			{
				new TraceEventConverter()
			}
		};
	}

	/// <summary>
	/// Initializes a new instance of <see cref="JsonStorageProvider"/> with custom serializer options.
	/// </summary>
	/// <param name="options">The custom serializer options to use.</param>
	public JsonStorageProvider( JsonSerializerOptions options )
	{
		options.Converters.Add( new TraceEventConverter() );
		Options = options;
	}

	/// <inheritdoc/>
	public override void Start()
	{
		CurrentTraceObject = new TraceObject();
		NamedThreads = new ConcurrentHashSet<int>();
	}

	/// <inheritdoc/>
	public override void Stop()
	{
		CurrentTraceObject = null;
		NamedThreads = null;
	}

	/// <inheritdoc/>
	public override void AddMetaData<T>( string key, T? value ) where T : default
	{
		if ( CurrentTraceObject!.MetaData.ContainsKey( key ) )
			throw new ArgumentException( $"A metadata entry with the key \"{key}\" already exists" );

		CurrentTraceObject.MetaData.AddOrUpdate( key, value, ( key, value ) => value );
	}

	/// <inheritdoc/>
	public override void AddEvent( TraceEvent traceEvent )
	{
		var threadId = ThreadSafe.CurrentThreadId;
		if ( !NamedThreads!.Contains( threadId ) )
		{
			NamedThreads.TryAdd( threadId );

			var threadName = ThreadSafe.CurrentThreadName ?? "UnknownThread";
			if ( threadId == 1 )
				threadName = "MainThread";

			CurrentTraceObject!.TraceEvents.Add( new TraceEvent
			{
				Name = "thread_name",
				Type = TraceType.Meta,
				ThreadId = threadId,
				Value = threadName
			} );
		}

		CurrentTraceObject!.TraceEvents.Add( traceEvent );
	}

	/// <inheritdoc/>
	public override void WriteToStream( Stream stream )
	{
		if ( !stream.CanWrite )
			throw new ArgumentException( "The stream cannot be written to", nameof( stream ) );
		
		JsonSerializer.Serialize( stream, new TraceObject( CurrentTraceObject! ), Options );
	}
}
