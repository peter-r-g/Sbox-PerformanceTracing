using PerformanceTracing.Providers.Json;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PerformanceTracing;

/// <summary>
/// Contains all options for configuring a trace.
/// </summary>
public sealed class TracingOptions
{
	/// <summary>
	/// The default options for tracing.
	/// </summary>
	public static readonly TracingOptions Default = new();

	/// <summary>
	/// Whether or not to append stack traces to all applicable events.
	/// </summary>
	public bool AppendStackTrace { get; set; } = false;
	/// <summary>
	/// Whether or not to append caller paths to applicable events.
	/// </summary>
	public bool AppendCallerPath { get; set; } = false;
	/// <summary>
	/// Whether or not to use simple method names.
	/// If true, this will replace method names with its fully qualified name in a stack trace.
	/// If false, this will just use the name of the method.
	/// </summary>
	/// <remarks>
	/// This only applies to method invocations that take a parameter marked with the <see cref="CallerMemberNameAttribute"/>.
	/// </remarks>
	public bool UseSimpleNames { get; set; } = false;

	/// <summary>
	/// Contains hard limits for how many of each type of trace can exist at once.
	/// </summary>
	/// <remarks>
	/// For <see cref="TraceType.Marker"/> and <see cref="TraceType.Meta"/> this has no effect.
	/// </remarks>
	public Dictionary<TraceType, int> MaxConcurrentTraces { get; } = new()
	{
		{ TraceType.Counter, 20 },
		{ TraceType.Performance, 50 }
	};

	/// <summary>
	/// The provider to use to store and serialize traces.
	/// </summary>
	public TraceStorageProvider StorageProvider { get; set; } = new JsonStorageProvider();

	/// <summary>
	/// Initializes a default instance of <see cref="TracingOptions"/>.
	/// </summary>
	public TracingOptions()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="TracingOptions"/> and clones all of the options in <paramref name="other"/>.
	/// </summary>
	/// <param name="other">The options to clone to the new instance.</param>
	public TracingOptions( TracingOptions other )
	{
		AppendStackTrace = other.AppendStackTrace;
		AppendCallerPath = other.AppendCallerPath;
		UseSimpleNames = other.UseSimpleNames;
		MaxConcurrentTraces = other.MaxConcurrentTraces;
	}

	/// <summary>
	/// Sets the <see cref="AppendStackTrace"/> option.
	/// </summary>
	/// <returns>The same options instance.</returns>
	public TracingOptions WithAppendStackTrace( bool appendStackTrace )
	{
		AppendStackTrace = appendStackTrace;
		return this;
	}

	/// <summary>
	/// Sets the <see cref="AppendCallerPath"/> option.
	/// </summary>
	/// <returns>The same options instance.</returns>
	public TracingOptions WithAppendCallerPath( bool appendCallerPath )
	{
		AppendCallerPath = appendCallerPath;
		return this;
	}

	/// <summary>
	/// Sets the <see cref="UseSimpleNames"/> option.
	/// </summary>
	/// <returns>The same options instance.</returns>
	public TracingOptions WithUseSimpleNames( bool useSimpleNames )
	{
		UseSimpleNames = useSimpleNames;
		return this;
	}

	/// <summary>
	/// Sets the <see cref="MaxConcurrentTraces"/> for the provided <see cref="TraceType"/>.
	/// </summary>
	/// <param name="type">The type of trace you are setting the limit for.</param>
	/// <param name="max">The maximum amount of traces that can exist at once.</param>
	/// <returns>The same options instance.</returns>
	public TracingOptions WithMaxConcurrentTraces( TraceType type, int max )
	{
		MaxConcurrentTraces[type] = max;
		return this;
	}

	/// <summary>
	/// Sets the <see cref="StorageProvider"/> option.
	/// </summary>
	/// <returns>The same options instance.</returns>
	public TracingOptions WithStorageProvider<T>() where T : TraceStorageProvider, new()
	{
		StorageProvider = new T();
		return this;
	}
}
