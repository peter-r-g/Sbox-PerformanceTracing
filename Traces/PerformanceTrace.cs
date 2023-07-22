using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PerformanceTracing.Traces;

/// <summary>
/// A trace that logs the duration of a block of code.
/// </summary>
public sealed class PerformanceTrace : IDisposable
{
	internal static ConcurrentQueue<PerformanceTrace> UnusedTraces { get; } = new();

	private string Name { get; set; } = string.Empty;
	private string Categories { get; set; } = "Uncategorized";
	private string? FilePath { get; set; }
	private int? LineNumber { get; set; }
	private string? StackTrace { get; set; }
	private long StartTicks { get; set; }

	private void Initialize( string name, IEnumerable<string> categories, string? filePath = null, int? lineNumber = null )
	{
		Name = name;
		if ( categories.Any() )
			Categories = string.Join( ',', categories );
		FilePath = filePath;
		LineNumber = lineNumber;
		if ( Tracing.IsRunning && Tracing.Options!.AppendStackTrace )
			StackTrace = StackTraceHelper.GetStackTrace( 2 );

		StartTicks = Stopwatch.GetTimestamp();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		var elapsedTime = Stopwatch.GetElapsedTime( StartTicks );
		if ( !Tracing.IsRunning )
			return;

		var startTime = Stopwatch.GetElapsedTime( Tracing.StartTimeTicks, StartTicks );
		var traceEvent = new TraceEvent
		{
			Name = Name,
			Categories = Categories,
			ThreadId = Tracing.ThreadId,
			EventType = "X",
			Timestamp = startTime.TotalMicroseconds,
			Duration = elapsedTime.TotalMicroseconds
		};

		if ( Tracing.Options!.AppendStackTrace )
			traceEvent.Arguments.Add( "stackTrace", StackTrace! );

		if ( Tracing.Options!.AppendCallerPath && FilePath is not null && LineNumber is not null )
		{
			var location = FilePath + ':' + LineNumber;
			traceEvent.Location = location;
			traceEvent.Arguments.Add( "location", location );
		}

		Tracing.AddEvent( traceEvent );

		UnusedTraces.Enqueue( this );
	}

	/// <summary>
	/// Creates a new <see cref="PerformanceTrace"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	public static PerformanceTrace New( string name )
	{
		if ( !Tracing.IsRunning )
			return new PerformanceTrace();

		if ( !UnusedTraces.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( PerformanceTrace )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Performance}]" );

		trace.Initialize( name, Array.Empty<string>() );
		return trace;
	}

	/// <summary>
	/// Creates a new <see cref="PerformanceTrace"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	/// <param name="categories">The categories to give the trace.</param>
	public static PerformanceTrace New( string name, IEnumerable<string> categories )
	{
		if ( !Tracing.IsRunning )
			return new PerformanceTrace();

		if ( !UnusedTraces.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( PerformanceTrace )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Performance}]" );

		trace.Initialize( name, categories );
		return trace;
	}

	/// <summary>
	/// Creates a new <see cref="PerformanceTrace"/>.
	/// </summary>
	/// <param name="name">Do not use.</param>
	/// <param name="filePath">Do not use.</param>
	/// <param name="lineNumber">Do not use.</param>
	public static PerformanceTrace New( [CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( !Tracing.IsRunning )
			return new PerformanceTrace();

		if ( !UnusedTraces.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( PerformanceTrace )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Performance}]" );

		if ( !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		trace.Initialize( name ?? "Unknown", Array.Empty<string>(), filePath, lineNumber );
		return trace;
	}

	/// <summary>
	/// Creates a new <see cref="PerformanceTrace"/>.
	/// </summary>
	/// <param name="categories">The categories to give the trace.</param>
	/// <param name="name">Do not use.</param>
	/// <param name="filePath">Do not use.</param>
	/// <param name="lineNumber">Do not use.</param>
	public static PerformanceTrace New( IEnumerable<string> categories,
		[CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( !Tracing.IsRunning )
			return new PerformanceTrace();

		if ( !UnusedTraces.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( PerformanceTrace )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Performance}]" );

		if ( !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		trace.Initialize( name ?? "Unknown", categories, filePath, lineNumber );
		return trace;
	}

	internal static void InitializeCache()
	{
		UnusedTraces.Clear();
		for ( var i = 0; i < Tracing.Options!.MaxConcurrentTraces[TraceType.Performance]; i++ )
			UnusedTraces.Enqueue( new PerformanceTrace() );
	}
}
