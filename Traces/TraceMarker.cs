using System.Diagnostics;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;

namespace PerformanceTracing.Traces;

/// <summary>
/// A trace that marks a point in time.
/// </summary>
public static class TraceMarker
{
	/// <summary>
	/// Adds a marker in the current trace.
	/// </summary>
	/// <param name="name">The name of the marker.</param>
	public static void New( string name )
	{
		AddMarker( name, Array.Empty<string>() );
	}

	/// <summary>
	/// Adds a marker in the current trace.
	/// </summary>
	/// <param name="name">The name of the marker.</param>
	/// <param name="categories">The categories to give the marker.</param>
	public static void New( string name, IEnumerable<string> categories )
	{
		AddMarker( name, categories );
	}

	/// <summary>
	/// Adds a marker in the current trace.
	/// </summary>
	/// <param name="name">Do not use.</param>
	/// <param name="filePath">Do not use.</param>
	/// <param name="lineNumber">Do not use.</param>
	public static void New( [CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( Tracing.IsRunning && !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		AddMarker( name ?? "Unknown", Array.Empty<string>(), filePath, lineNumber );
	}

	/// <summary>
	/// Adds a marker in the current trace.
	/// </summary>
	/// <param name="categories">The categories to give the marker.</param>
	/// <param name="name">Do not use.</param>
	/// <param name="filePath">Do not use.</param>
	/// <param name="lineNumber">Do not use.</param>
	public static void New( IEnumerable<string> categories,
		[CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( Tracing.IsRunning && !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		AddMarker( name ?? "Unknown", categories, filePath, lineNumber );
	}

	/// <summary>
	/// Adds a marker in the current trace.
	/// </summary>
	/// <param name="name">The name of the method invoking this method.</param>
	/// <param name="categories">The categories to give the marker.</param>
	/// <param name="filePath">The file path to the invoker.</param>
	/// <param name="lineNumber">The line number of the invoker.</param>
	/// <exception cref="InvalidOperationException">Thrown when trying to add a marker when no trace is running.</exception>
	private static void AddMarker( string name, IEnumerable<string> categories, string? filePath = null, int? lineNumber = null )
	{
		var startTicks = Stopwatch.GetTimestamp();
		if ( !Tracing.IsRunning )
			throw new InvalidOperationException( "There is no trace running" );

		if ( !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		var startTime = Stopwatch.GetElapsedTime( Tracing.StartTime.Ticks, startTicks );
		var traceEvent = new TraceEvent
		{
			Name = name ?? "Unknown",
			Categories = string.Join( ',', categories ),
			ThreadId = Tracing.ThreadId,
			EventType = "R",
			Timestamp = startTime.TotalMicroseconds
		};

		if ( Tracing.Options!.AppendStackTrace )
			traceEvent.Arguments.Add( "stackTrace", StackTraceHelper.GetStackTrace( 2 ) );

		if ( Tracing.Options.AppendFilePath && filePath is not null )
			traceEvent.Arguments.Add( "filePath", filePath );

		if ( Tracing.Options.AppendLineNumber && lineNumber is not null )
			traceEvent.Arguments.Add( "lineNumber", lineNumber.Value.ToString() );

		Tracing.Events.Add( traceEvent );
	}
}
