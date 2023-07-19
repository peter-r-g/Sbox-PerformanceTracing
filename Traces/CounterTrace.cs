using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PerformanceTracing.Traces;

/// <summary>
/// A trace that logs changes to a value.
/// </summary>
public readonly struct CounterTrace : IDisposable
{
	private string Name { get; }
	private string Categories { get; }
	private string? FilePath { get; }
	private int? LineNumber { get; }
	private string? StackTrace { get; }
	private long StartTicks { get; }

	/// <summary>
	/// Do not use.
	/// </summary>
	/// <exception cref="InvalidOperationException">Always thrown.</exception>
	public CounterTrace()
	{
		throw new InvalidOperationException( $"Use one of the {nameof( CounterTrace )} static constructors" );
	}

	private CounterTrace( string name, IEnumerable<string> categories, object? initialValue, string? filePath = null, int? lineNumber = null )
	{
		Name = name;
		Categories = string.Join( ',', categories );
		FilePath = filePath;
		LineNumber = lineNumber;
		if ( Tracing.IsRunning && Tracing.Options!.AppendStackTrace )
			StackTrace = StackTraceHelper.GetStackTrace( 2 );

		StartTicks = Stopwatch.GetTimestamp();
		Update( initialValue );
	}

	/// <summary>
	/// Updates the value of the trace.
	/// </summary>
	/// <param name="newValue">The new value of the trace.</param>
	public void Update( object? newValue )
	{
		if ( !Tracing.IsRunning )
			return;

		var elapsedTime = Stopwatch.GetElapsedTime( StartTicks ) + Tracing.StartTime;
		var traceEvent = new TraceEvent
		{
			Name = Name,
			Categories = Categories,
			ThreadId = Tracing.ThreadId,
			EventType = "C",
			Timestamp = elapsedTime.TotalMicroseconds,
			Arguments =
			{
				{ "value", newValue?.ToString() ?? "null" }
			}
		};

		if ( Tracing.Options!.AppendStackTrace )
			traceEvent.Arguments.Add( "stackTrace", StackTrace! );

		if ( Tracing.Options.AppendFilePath && FilePath is not null )
			traceEvent.Arguments.Add( "filePath", FilePath );

		if ( Tracing.Options.AppendLineNumber && LineNumber is not null )
			traceEvent.Arguments.Add( "lineNumber", LineNumber.Value.ToString() );

		Tracing.Events.Add( traceEvent );
	}

	/// <inheritdoc/>
	public readonly void Dispose()
	{
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <returns></returns>
	public static CounterTrace New( string name, object? initialValue )
	{
		return new( name, Array.Empty<string>(), initialValue );
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	/// <param name="categories">The categories to give the trace.</param>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <returns></returns>
	public static CounterTrace New( string name, IEnumerable<string> categories, object? initialValue )
	{
		return new( name, categories, initialValue );
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace"/>.
	/// </summary>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <param name="name">Do not use.</param>
	/// <param name="filePath">Do not use.</param>
	/// <param name="lineNumber">Do not use.</param>
	/// <returns></returns>
	public static CounterTrace New( object? initialValue,
		[CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( Tracing.IsRunning && !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		return new( name ?? "Unknown", Array.Empty<string>(), initialValue, filePath, lineNumber );
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
	public static CounterTrace New( IEnumerable<string> categories, object? initialValue,
		[CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( Tracing.IsRunning && !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		return new( name ?? "Unknown", categories, initialValue, filePath, lineNumber );
	}
}
