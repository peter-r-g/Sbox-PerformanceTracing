﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PerformanceTracing.Traces;

/// <summary>
/// A trace that logs changes to a value.
/// </summary>
/// <typeparam name="T">The type of value to change.</typeparam>
public struct CounterTrace<T> : IDisposable
{
	private string Name { get; }
	private string Categories { get; }
	private string? FilePath { get; }
	private int? LineNumber { get; }
	private string? StackTrace { get; }
	private long StartTicks { get; }

	private T? LastValue { get; set; } = default;

	/// <summary>
	/// Do not use.
	/// </summary>
	/// <exception cref="InvalidOperationException">Always thrown.</exception>
	public CounterTrace()
	{
		throw new InvalidOperationException( $"Use one of the {nameof( CounterTrace<T> )} static constructors" );
	}

	private CounterTrace( string name, IEnumerable<string> categories, T? initialValue, string? filePath = null, int? lineNumber = null )
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
	public void Update( T? newValue )
	{
		if ( !Tracing.IsRunning || EqualityComparer<T?>.Default.Equals( LastValue, newValue ) )
			return;

		LastValue = newValue;

		var elapsedTime = Stopwatch.GetElapsedTime( Tracing.StartTime.Ticks, Stopwatch.GetTimestamp() );
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
	/// Creates a new <see cref="CounterTrace{T}"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <returns></returns>
	public static CounterTrace<T> New( string name, T? initialValue )
	{
		return new( name, Array.Empty<string>(), initialValue );
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace{T}"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	/// <param name="categories">The categories to give the trace.</param>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <returns></returns>
	public static CounterTrace<T> New( string name, IEnumerable<string> categories, T? initialValue )
	{
		return new( name, categories, initialValue );
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace{T}"/>.
	/// </summary>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <param name="name">Do not use.</param>
	/// <param name="filePath">Do not use.</param>
	/// <param name="lineNumber">Do not use.</param>
	/// <returns></returns>
	public static CounterTrace<T> New( T? initialValue,
		[CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( Tracing.IsRunning && !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		return new( name ?? "Unknown", Array.Empty<string>(), initialValue, filePath, lineNumber );
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace{T}"/>.
	/// </summary>
	/// <param name="categories">The categories to give the trace.</param>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <param name="name">Do not use.</param>
	/// <param name="filePath">Do not use.</param>
	/// <param name="lineNumber">Do not use.</param>
	/// <returns></returns>
	public static CounterTrace<T> New( IEnumerable<string> categories, T? initialValue,
		[CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( Tracing.IsRunning && !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		return new( name ?? "Unknown", categories, initialValue, filePath, lineNumber );
	}
}