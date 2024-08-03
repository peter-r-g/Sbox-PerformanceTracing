using Sandbox;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
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
	private static readonly PerformanceTrace disabledTrace = new();

	private string Name { get; set; } = string.Empty;
	private ImmutableArray<string> Categories { get; set; } = ImmutableArray.Create( "Uncategorized" );
	private string? FilePath { get; set; }
	private int? LineNumber { get; set; }
	private string? StackTrace { get; set; }
	private long StartTicks { get; set; }

	private void Initialize( string name, IEnumerable<string> categories, string? filePath = null, int? lineNumber = null )
	{
		Name = name;
		if ( categories.Any() )
			Categories = categories.ToImmutableArray();
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
		if ( !Tracing.IsRunning || ReferenceEquals( this, disabledTrace ) )
			return;

		var startTime = Stopwatch.GetElapsedTime( Tracing.StartTimeTicks, StartTicks );
		var traceEvent = new TraceEvent
		{
			Name = Name,
			Categories = Categories,
			ThreadId = ThreadSafe.CurrentThreadId,
			Type = TraceType.Performance,
			Timestamp = startTime.TotalNanoseconds,
			Duration = elapsedTime.TotalNanoseconds
		};

		if ( Tracing.Options!.AppendStackTrace )
			traceEvent.StackTrace = StackTrace!;

		if ( Tracing.Options!.AppendCallerPath && FilePath is not null && LineNumber is not null )
			traceEvent.Location = new SourceLocation( FilePath, LineNumber.Value );

		Tracing.AddEvent( traceEvent );

		UnusedTraces.Enqueue( this );
	}

	/// <summary>
	/// Creates a new <see cref="PerformanceTrace"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	/// <exception cref="InvalidOperationException">Thrown when exceeding the maximum amount of traces stored in the pool.</exception>
	public static PerformanceTrace New( string name )
	{
		if ( !Tracing.IsRunning )
			return disabledTrace;

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
	/// <exception cref="InvalidOperationException">Thrown when exceeding the maximum amount of traces stored in the pool.</exception>
	public static PerformanceTrace New( string name, IEnumerable<string> categories )
	{
		if ( !Tracing.IsRunning )
			return disabledTrace;

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
	/// <exception cref="InvalidOperationException">Thrown when exceeding the maximum amount of traces stored in the pool.</exception>
	public static PerformanceTrace New( [CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( !Tracing.IsRunning )
			return disabledTrace;

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
	/// <exception cref="InvalidOperationException">Thrown when exceeding the maximum amount of traces stored in the pool.</exception>
	public static PerformanceTrace New( IEnumerable<string> categories,
		[CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( !Tracing.IsRunning )
			return disabledTrace;

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
