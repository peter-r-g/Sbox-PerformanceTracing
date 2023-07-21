using PerformanceTracing.Traces;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Concurrent;

namespace PerformanceTracing;

/// <summary>
/// Provides logic to run performance tracing in your program.
/// </summary>
public static class Tracing
{
	/// <summary>
	/// Whether or not a trace is currently running.
	/// </summary>
	public static bool IsRunning { get; private set; }

	internal static TracingOptions? Options { get; private set; }
	internal static TimeSpan StartTime => CurrentTraceObject!.StartTime;
	internal static ConcurrentBag<TraceEvent> Events => CurrentTraceObject!.TraceEvents;
	private static TraceObject? CurrentTraceObject { get; set; }

	internal static int ThreadId => ThreadSafe.IsMainThread ? 1 : 2;

	/// <summary>
	/// Starts a new trace. If one is already running, it is overwritten.
	/// </summary>
	/// <param name="options">The options to provide for the trace.</param>
	public static void Start( TracingOptions? options = null )
	{
		Options = new TracingOptions( options ?? TracingOptions.Default );

		PerformanceTrace.InitializeCache();

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

		IsRunning = true;
	}

	/// <summary>
	/// Stops any active trace.
	/// </summary>
	public static void Stop()
	{
		CurrentTraceObject = default;
		Options = null;
		IsRunning = false;
	}

	/// <summary>
	/// Adds some metadata to the currently running trace.
	/// </summary>
	/// <typeparam name="T">The type of the value to add to metadata.</typeparam>
	/// <param name="key">The key to insert the metadata at.</param>
	/// <param name="value">The value to store.</param>
	/// <exception cref="InvalidOperationException">Thrown when trying to add metadata when no trace is running.</exception>
	/// <exception cref="ArgumentException">Thrown when </exception>
	public static void AddMetaData<T>( string key, T? value )
	{
		if ( !IsRunning )
			throw new InvalidOperationException( "There is no trace running" );

		if ( CurrentTraceObject!.MetaData.ContainsKey( key ) )
			throw new ArgumentException( $"A metadata entry with the key \"{key}\" already exists" );

		CurrentTraceObject.MetaData.Add( key, value );
	}

	#region Save Methods
	/// <summary>
	/// Copies all trace data to the clients clipboard.
	/// </summary>
	/// <param name="stopTracing">Whether or not to stop tracing after copying the trace data to clipboard.</param>
	/// <exception cref="InvalidOperationException">Thrown when trying to copy to clipboard when no trace is running.</exception>
	/// <exception cref="Exception">Thrown when invoking this method from outside the client realm.</exception>
	public static void CopyToClipboard( bool stopTracing = false )
	{
		if ( !IsRunning )
			throw new InvalidOperationException( "There is no trace running" );

		Game.AssertClient();
		Clipboard.SetText( Json.Serialize( CurrentTraceObject ) );

		if ( stopTracing )
			Stop();
	}

	/// <summary>
	/// Saves all trace data to a file in the given <see cref="FileSystem"/>.
	/// </summary>
	/// <param name="filePath">The path to the file to save.</param>
	/// <param name="fs">The file system to save into. Defaults to <see cref="FileSystem.Data"/>.</param>
	/// <param name="stopTracing">Whether or not to stop tracing after saving the trace data to the file.</param>
	/// <exception cref="InvalidOperationException">Thrown when invoking this method from outside the client realm.</exception>
	public static void SaveToFile( string filePath, BaseFileSystem? fs = null, bool stopTracing = false )
	{
		if ( !IsRunning )
			throw new InvalidOperationException( "There is no trace running" );

		fs ??= FileSystem.Data;
		fs.WriteJson( filePath, CurrentTraceObject );

		if ( stopTracing )
			Stop();
	}
	#endregion
}
