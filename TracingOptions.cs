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
}
