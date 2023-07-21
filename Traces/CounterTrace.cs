using Sandbox;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PerformanceTracing.Traces;

/// <summary>
/// A trace that logs changes to a value.
/// </summary>
/// <typeparam name="T">The type of value to change.</typeparam>
public sealed class CounterTrace<T> : IDisposable where T : notnull, INumber<T>
{
#pragma warning disable SB3000 // Hotloading not supported
	internal static ConcurrentQueue<CounterTrace<T>>? UnusedTraces { get; set; }
#pragma warning restore SB3000 // Hotloading not supported

	private string Name { get; set; } = string.Empty;
	private string Categories { get; set; } = "Uncategorized";
	private string? FilePath { get; set; }
	private int? LineNumber { get; set; }

	private T LastValue { get; set; } = default!;

	private void Initialize( string name, IEnumerable<string> categories, T initialValue, string? filePath = null, int? lineNumber = null )
	{
		Name = name;
		if ( categories.Any() )
			Categories = string.Join( ',', categories );
		FilePath = filePath;
		LineNumber = lineNumber;

		LastValue = initialValue;
		WriteEvent();
	}

	/// <summary>
	/// Updates the value of the trace.
	/// </summary>
	/// <param name="newValue">The new value of the trace.</param>
	public void Update( T newValue )
	{
		if ( !Tracing.IsRunning )
			return;

		if ( EqualityComparer<T>.Default.Equals( LastValue, newValue ) )
			return;

		LastValue = newValue;
		WriteEvent();
	}

	private void WriteEvent()
	{
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
				{ Name, LastValue }
			}
		};

		if ( Tracing.Options!.AppendCallerPath && FilePath is not null && LineNumber is not null )
			traceEvent.Location = FilePath + ':' + LineNumber;

		Tracing.Events.Add( traceEvent );
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if ( UnusedTraces is null )
			InitializeCache();

		UnusedTraces!.Enqueue( this );
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace{T}"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <returns></returns>
	public static CounterTrace<T> New( string name, T initialValue )
	{
		if ( !Tracing.IsRunning )
			return new CounterTrace<T>();

		if ( UnusedTraces is null )
			InitializeCache();

		if ( !UnusedTraces!.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( CounterTrace<T> )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Counter}]" );

		trace.Initialize( name, Array.Empty<string>(), initialValue );
		return trace;
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace{T}"/>.
	/// </summary>
	/// <param name="name">The name of the trace.</param>
	/// <param name="categories">The categories to give the trace.</param>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <returns></returns>
	public static CounterTrace<T> New( string name, IEnumerable<string> categories, T initialValue )
	{
		if ( !Tracing.IsRunning )
			return new CounterTrace<T>();

		if ( UnusedTraces is null )
			InitializeCache();

		if ( !UnusedTraces!.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( CounterTrace<T> )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Counter}]" );

		trace.Initialize( name, categories, initialValue );
		return trace;
	}

	/// <summary>
	/// Creates a new <see cref="CounterTrace{T}"/>.
	/// </summary>
	/// <param name="initialValue">The first value to log for the trace.</param>
	/// <param name="name">Do not use.</param>
	/// <param name="filePath">Do not use.</param>
	/// <param name="lineNumber">Do not use.</param>
	/// <returns></returns>
	public static CounterTrace<T> New( T initialValue,
		[CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( !Tracing.IsRunning )
			return new CounterTrace<T>();

		if ( UnusedTraces is null )
			InitializeCache();

		if ( !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		if ( !UnusedTraces!.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( CounterTrace<T> )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Counter}]" );

		trace.Initialize( name ?? "Unknown", Array.Empty<string>(), initialValue, filePath, lineNumber );
		return trace;
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
	public static CounterTrace<T> New( IEnumerable<string> categories, T initialValue,
		[CallerMemberName] string? name = null,
		[CallerFilePath] string? filePath = null,
		[CallerLineNumber] int? lineNumber = null )
	{
		if ( !Tracing.IsRunning )
			return new CounterTrace<T>();

		if ( UnusedTraces is null )
			InitializeCache();

		if ( !Tracing.Options!.UseSimpleNames )
			name = StackTraceHelper.GetTraceEntrySignature( 1 );

		if ( !UnusedTraces!.TryDequeue( out var trace ) )
			throw new InvalidOperationException( $"The {nameof( CounterTrace<T> )} pool has been exhausted. Consider upping {nameof( TracingOptions.MaxConcurrentTraces )}[{TraceType.Counter}]" );

		trace.Initialize( name ?? "Unknown", categories, initialValue, filePath, lineNumber );
		return trace;
	}

	[Event.Hotload]
	private static void InitializeCache()
	{
		if ( !Tracing.IsRunning )
			return;

		UnusedTraces = new();
		for ( var i = 0; i < Tracing.Options!.MaxConcurrentTraces[TraceType.Counter]; i++ )
			UnusedTraces.Enqueue( new CounterTrace<T>() );
	}
}
