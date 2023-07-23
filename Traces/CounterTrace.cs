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
/// A trace that logs changes to a value.
/// </summary>
public sealed class CounterTrace : IDisposable
{
	internal static ConcurrentQueue<CounterTrace> UnusedTraces { get; } = new();

	private string Name { get; set; } = string.Empty;
	private ImmutableArray<string> Categories { get; set; } = ImmutableArray.Create( "Uncategorized" );
	private string? FilePath { get; set; }
	private int? LineNumber { get; set; }

	private double LastValue { get; set; }

	private void Initialize( string name, IEnumerable<string> categories, double initialValue, string? filePath = null, int? lineNumber = null )
	{
		Name = name;
		if ( categories.Any() )
			Categories = categories.ToImmutableArray();
		FilePath = filePath;
		LineNumber = lineNumber;

		LastValue = initialValue;
		WriteEvent();
	}

	/// <summary>
	/// Updates the value of the trace.
	/// </summary>
	/// <param name="newValue">The new value of the trace.</param>
	public void Update( double newValue )
	{
		if ( !Tracing.IsRunning )
			return;

		if ( LastValue == newValue )
			return;

		LastValue = newValue;
		WriteEvent();
	}

	private void WriteEvent()
	{
		var elapsedTime = Stopwatch.GetElapsedTime( Tracing.StartTimeTicks, Stopwatch.GetTimestamp() );
		var traceEvent = new TraceEvent
		{
			Name = Name,
			Categories = Categories,
			ThreadId = Tracing.ThreadId,
			Type = TraceType.Counter,
			Timestamp = elapsedTime.TotalNanoseconds,
			Value = LastValue
		};

		if ( Tracing.Options!.AppendCallerPath && FilePath is not null && LineNumber is not null )
			traceEvent.Location = new SourceLocation( FilePath, LineNumber.Value );

		Tracing.AddEvent( traceEvent );
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		UnusedTraces.Enqueue( this );
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <returns></returns>
	public static CounterTrace New( string name, double initialValue )
	{
		if ( !Tracing.IsRunning )
			return new CounterTrace();

		if ( !UnusedTraces!.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( CounterTrace )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Counter}]" );

		trace.Initialize( name, Array.Empty<string>(), initialValue );
		return trace;
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	/// <param name="categories">The categories to give the trace.</param>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <returns></returns>
	public static CounterTrace New( string name, IEnumerable<string> categories, double initialValue )
	{
		if ( !Tracing.IsRunning )
			return new CounterTrace();

		if ( !UnusedTraces!.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( CounterTrace )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Counter}]" );

		trace.Initialize( name, categories, initialValue );
		return trace;
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace"/>.
	/// </summary>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <param name="name">Do not use.</param>
	/// <param name="filePath">Do not use.</param>
	/// <param name="lineNumber">Do not use.</param>
	/// <returns></returns>
	public static CounterTrace New( double initialValue,
		[CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( !Tracing.IsRunning )
			return new CounterTrace();

		if ( !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		if ( !UnusedTraces!.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( CounterTrace )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Counter}]" );

		trace.Initialize( name ?? "Unknown", Array.Empty<string>(), initialValue, filePath, lineNumber );
		return trace;
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace"/>.
	/// </summary>
	/// <param name="categories">The categories to give the trace.</param>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <param name="name">Do not use.</param>
	/// <param name="filePath">Do not use.</param>
	/// <param name="lineNumber">Do not use.</param>
	/// <returns></returns>
	public static CounterTrace New( IEnumerable<string> categories, double initialValue,
		[CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( !Tracing.IsRunning )
			return new CounterTrace();

		if ( !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		if ( !UnusedTraces!.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( CounterTrace )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Counter}]" );

		trace.Initialize( name ?? "Unknown", categories, initialValue, filePath, lineNumber );
		return trace;
	}

	internal static void InitializeCache()
	{
		UnusedTraces.Clear();
		for ( var i = 0; i < Tracing.Options!.MaxConcurrentTraces[TraceType.Counter]; i++ )
			UnusedTraces.Enqueue( new CounterTrace() );
	}
}
