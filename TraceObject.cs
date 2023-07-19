using Sandbox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace PerformanceTracing;

internal sealed class TraceObject
{
	[JsonPropertyName( "traceEvents" )]
	public List<TraceEvent> TraceEvents { get; set; } = new();
	[JsonPropertyName( "otherData" )]
	public Dictionary<string, object?> OtherData { get; init; } = new()
	{
		{ "perfTracingVersion", "1.0.0" },
	};
	internal TimeSpan StartTime { get; }

	public TraceObject()
	{
		if ( Game.IsServer )
		{
			OtherData.Add( "realm", "server" );
			OtherData.Add( "serverSteamid", Game.ServerSteamId );
			OtherData.Add( "isDedicatedServer", Game.IsDedicatedServer );
			OtherData.Add( "tickRate", Game.TickRate );
		}
		else if ( Game.IsClient )
		{
			OtherData.Add( "realm", "client" );
			OtherData.Add( "steamid", Game.SteamId );
			OtherData.Add( "isServerHost", Game.IsServerHost );
			OtherData.Add( "tickRate", Game.TickRate );
		}
		// Menu.
		else
		{
			OtherData.Add( "realm", "menu" );
		}

		if ( Game.IsDedicatedServer )
			return;

		OtherData.Add( "isEditorRunning", Game.IsEditor );
		OtherData.Add( "isHandheld", Game.IsRunningOnHandheld );
		OtherData.Add( "isVr", Game.IsRunningInVR );

		StartTime = Stopwatch.GetElapsedTime( 0 );
	}
}
