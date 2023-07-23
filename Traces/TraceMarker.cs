using System.Diagnostics;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

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
		if ( !Tracing.IsRunning )
			return;

		AddMarker( name, Array.Empty<string>() );
	}

	/// <summary>
	/// Adds a marker in the current trace.
	/// </summary>
	/// <param name="name">The name of the marker.</param>
	/// <param name="categories">The categories to give the marker.</param>
	public static void New( string name, IEnumerable<string> categories )
	{
		if ( !Tracing.IsRunning )
			return;

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
		if ( !Tracing.IsRunning )
			return;

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
		if ( !Tracing.IsRunning )
			return;

		if ( !Tracing.Options!.UseSimpleNames )
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
		var startTime = Stopwatch.GetElapsedTime( Tracing.StartTimeTicks, startTicks );
		var traceEvent = new TraceEvent
		{
			Name = name ?? "Unknown",
			Categories = categories.Any() ? categories.ToImmutableArray() : ImmutableArray.Create( "Uncategorized" ),
			ThreadId = Tracing.ThreadId,
			Type = TraceType.Marker,
			Timestamp = startTime.TotalNanoseconds
		};

		if ( Tracing.Options!.AppendStackTrace )
			traceEvent.StackTrace = StackTraceHelper.GetStackTrace( 2 );

		if ( Tracing.Options!.AppendCallerPath && filePath is not null && lineNumber is not null )
			traceEvent.Location = new SourceLocation( filePath, lineNumber.Value );

		Tracing.AddEvent( traceEvent );
	}
}
