using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PerformanceTracing.Traces;

/// <summary>
/// A trace that logs the duration of a block of code.
/// </summary>
public readonly struct PerformanceTrace : IDisposable
{
	private string Name { get; }
	private string Categories { get; }
	private string? FilePath { get; }
	private int? LineNumber { get; } = null;
	private string? StackTrace { get; }
	private long StartTicks { get; }

	/// <summary>
	/// Do not use.
	/// </summary>
	/// <exception cref="InvalidOperationException">Always thrown.</exception>
	public PerformanceTrace()
	{
		throw new InvalidOperationException( $"Use one of the {nameof( PerformanceTrace )} static constructors" );
	}

	private PerformanceTrace( string name, IEnumerable<string> categories, string? filePath = null, int? lineNumber = null )
	{
		Name = name;
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

		var startTime = Stopwatch.GetElapsedTime( Tracing.StartTime.Ticks, StartTicks );
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

		if ( Tracing.Options.AppendFilePath && FilePath is not null )
			traceEvent.Arguments.Add( "filePath", FilePath );

		if ( Tracing.Options.AppendLineNumber && LineNumber is not null )
			traceEvent.Arguments.Add( "lineNumber", LineNumber.Value.ToString() );

		Tracing.Events.Add( traceEvent );
	}

	/// <summary>
	/// Creates a new <see cref="PerformanceTrace"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	public static PerformanceTrace New( string name )
	{
		return new( name, Array.Empty<string>() );
	}

	/// <summary>
	/// Creates a new <see cref="PerformanceTrace"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	/// <param name="categories">The categories to give the trace.</param>
	public static PerformanceTrace New( string name, IEnumerable<string> categories )
	{
		return new( name, categories );
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
		if ( Tracing.IsRunning && !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		return new( name ?? "Unknown", Array.Empty<string>(), filePath, lineNumber );
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
		if ( Tracing.IsRunning && !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		return new( name ?? "Unknown", categories, filePath, lineNumber );
	}
}
