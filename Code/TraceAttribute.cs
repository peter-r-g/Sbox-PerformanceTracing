using PerformanceTracing.Traces;
using Sandbox;
using System;

namespace PerformanceTracing;

[AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
[CodeGenerator( CodeGeneratorFlags.WrapPropertyGet |
	CodeGeneratorFlags.WrapPropertySet |
	CodeGeneratorFlags.WrapMethod |
	CodeGeneratorFlags.Static |
	CodeGeneratorFlags.Instance,
	"TraceStaticHandlers.Wrapped" )]
public sealed class TraceAttribute : Attribute
{
}

internal static class TraceStaticHandlers
{
	internal static void Wrapped<T>( WrappedPropertySet<T> p )
	{
		using var _ = PerformanceTrace.New( p.PropertyName + "_Set" );
		p.Setter( p.Value );
	}

	internal static T Wrapped<T>( WrappedPropertyGet<T> p )
	{
		using var _ = PerformanceTrace.New( p.PropertyName + "_Get" );
		return p.Value;
	}

	internal static void Wrapped( WrappedMethod m, params object[] args )
	{
		using var _ = PerformanceTrace.New( m.MethodName );
		m.Resume();
	}

	internal static T Wrapped<T>( WrappedMethod<T> m, params object[] args )
	{
		using var _ = PerformanceTrace.New( m.MethodName );
		return m.Resume();
	}
}
