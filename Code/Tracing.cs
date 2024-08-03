using PerformanceTracing.Traces;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text;

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

	/// <summary>
	/// The current version of the Performance Tracing library.
	/// </summary>
	public const string Version = "2.0.0";

	internal static readonly ImmutableArray<string> DefaultCategories = ImmutableArray.Create( "Uncategorized" );

	internal static TracingOptions? Options { get; private set; }
	internal static long StartTimeTicks { get; private set; }

	/// <summary>
	/// Starts a new trace. If one is already running, it is overwritten.
	/// </summary>
	/// <param name="options">The options to provide for the trace.</param>
	public static void Start( TracingOptions? options = null )
	{
		Options = new TracingOptions( options ?? TracingOptions.Default );

		CounterTrace.InitializeCache();
		PerformanceTrace.InitializeCache();

		IsRunning = true;
		StartTimeTicks = Stopwatch.GetTimestamp();
		Options.StorageProvider.Start();
		AddDefaultMetaData();
	}
	
	private static void AddDefaultMetaData()
	{
		AddMetaData( "perfTracingVersion", Version );

		AddMetaData( "appid", Game.AppId );
		AddMetaData( "ident", Game.Ident );
		AddMetaData( "steamid", Game.SteamId );

		AddMetaData( "isEditorRunning", Game.IsEditor );
		AddMetaData( "isHandheld", Game.IsRunningOnHandheld );
		AddMetaData( "isVr", Game.IsRunningInVR );
	}

	/// <summary>
	/// Stops any active trace.
	/// </summary>
	public static void Stop()
	{
		Options?.StorageProvider.Stop();
		Options = null;
		IsRunning = false;
	}

	/// <summary>
	/// Adds some metadata to the currently running trace.
	/// </summary>
	/// <typeparam name="T">The type of the value to add to metadata.</typeparam>
	/// <param name="key">The key to insert the metadata at.</param>
	/// <param name="value">The value to store.</param>
	public static void AddMetaData<T>( string key, T? value )
	{
		if ( !IsRunning )
			throw new InvalidOperationException( "There is no trace running" );

		Options!.StorageProvider.AddMetaData( key, value );
	}

	internal static void AddEvent( in TraceEvent traceEvent )
	{
		Options!.StorageProvider.AddEvent( traceEvent );
	}

	#region Save Methods
	/// <summary>
	/// Copies all trace data to the clients clipboard.
	/// </summary>
	/// <param name="stopTracing">Whether or not to stop tracing after copying the trace data to clipboard.</param>
	/// <exception cref="InvalidOperationException">Thrown when trying to copy to clipboard when no trace is running.</exception>
	/// <exception cref="NotSupportedException">Thrown when the storage provider does not support writing data to a stream.</exception>
	public static void CopyToClipboard( bool stopTracing = false )
	{
		if ( !IsRunning )
			throw new InvalidOperationException( "There is no trace running" );

		using ( var stream = new MemoryStream() )
		{
			Options!.StorageProvider.WriteToStream( stream );
			Clipboard.SetText( Encoding.UTF8.GetString( stream.ToArray() ) );
		}

		if ( stopTracing )
			Stop();
	}

	/// <summary>
	/// Saves all trace data to a file in the given <see cref="FileSystem"/>.
	/// </summary>
	/// <param name="filePath">The path to the file to save.</param>
	/// <param name="fs">The file system to save into. Defaults to <see cref="FileSystem.Data"/>.</param>
	/// <param name="stopTracing">Whether or not to stop tracing after saving the trace data to the file.</param>
	/// <exception cref="InvalidOperationException">Thrown when trying to save while there is no trace running.</exception>
	/// <exception cref="NotSupportedException">Thrown when the storage provider does not support writing data to a stream.</exception>
	public static void SaveToFile( string filePath, BaseFileSystem? fs = null, bool stopTracing = false )
	{
		if ( !IsRunning )
			throw new InvalidOperationException( "There is no trace running" );

		fs ??= FileSystem.Data;
		using ( var stream = fs.OpenWrite( filePath ) )
			Options!.StorageProvider.WriteToStream( stream );

		if ( stopTracing )
			Stop();
	}

	/// <summary>
	/// Gets a stream of trace data from the storage provider.
	/// </summary>
	/// <param name="stopTracing">Whether or not to stop tracing after retreiving the stream.</param>
	/// <returns>An in-memory store of the data.</returns>
	/// <exception cref="InvalidOperationException">Thrown when trying to get a stream while there is no trace running.</exception>
	/// <exception cref="NotSupportedException">Thrown when the storage provider does not support writing data to a stream.</exception>
	public static MemoryStream GetStream( bool stopTracing )
	{
		if ( !IsRunning )
			throw new InvalidOperationException( "There is no trace running" );

		var stream = new MemoryStream();
		Options!.StorageProvider.WriteToStream( stream );

		if ( stopTracing )
			Stop();

		return stream;
	}
	#endregion
}
