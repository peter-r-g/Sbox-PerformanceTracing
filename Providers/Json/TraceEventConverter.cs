using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PerformanceTracing.Providers.Json;

internal sealed class TraceEventConverter : JsonConverter<TraceEvent>
{
	public override TraceEvent Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		throw new NotSupportedException();
	}

	public override void Write( Utf8JsonWriter writer, TraceEvent value, JsonSerializerOptions options )
	{
		writer.WriteStartObject();

		writer.WriteString( "name", value.Name );
		writer.WriteString( "cat", string.Join( ',', value.Categories ) );
		writer.WriteString( "ph", value.Type.ToChromeTraceFormat() );
		writer.WriteNumber( "pid", 1 );
		writer.WriteNumber( "tid", value.ThreadId );

		// Tracy compatability.
		if ( value.Location != SourceLocation.None )
			writer.WriteString( "loc", value.Location.ToString() );

		writer.WriteNumber( "ts", value.Timestamp );
		if ( value.Duration.HasValue )
			writer.WriteNumber( "dur", value.Duration.Value );

		switch ( value.Type )
		{
			case TraceType.Counter:
				writer.WriteStartObject( "args" );
				writer.WriteNumber( value.Name, value.ValueNumber );
				writer.WriteEndObject();
				break;
			case TraceType.Meta:
				writer.WriteStartObject( "args" );
				writer.WriteString( "name", value.ValueString );
				writer.WriteEndObject();
				break;
			case TraceType.Marker:
			case TraceType.Performance:
				if ( value.Location == SourceLocation.None && value.StackTrace is null )
					break;

				writer.WriteStartObject( "args" );

				if ( value.Location != SourceLocation.None )
					writer.WriteString( "location", value.Location.ToString() );

				if ( value.StackTrace is not null )
					writer.WriteString( "stackTrace", value.StackTrace );

				writer.WriteEndObject();
				break;
		}

		writer.WriteEndObject();
	}
}
