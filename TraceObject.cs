using Sandbox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace PerformanceTracing;

internal struct TraceObject
{
	[JsonPropertyName( "traceEvents" )]
	public List<TraceEvent> TraceEvents { get; set; } = new();
	[JsonPropertyName( "otherData" )]
	public Dictionary<string, string> OtherData { get; init; } = new()
	{
		{ "perfTracingVersion", "1.0.0" },
	};
	internal TimeSpan StartTime { get; }

	public TraceObject()
	{
		if ( Game.IsServer )
		{
			OtherData.Add( "realm", "server" );
			OtherData.Add( "serverSteamid", Game.ServerSteamId.ToString() );
			OtherData.Add( "isDedicatedServer", Game.IsDedicatedServer.ToString() );
			OtherData.Add( "tickRate", Game.TickRate.ToString() );
		}
		else if ( Game.IsClient )
		{
			OtherData.Add( "realm", "client" );
			OtherData.Add( "steamid", Game.SteamId.ToString() );
			OtherData.Add( "isServerHost", Game.IsServerHost.ToString() );
			OtherData.Add( "tickRate", Game.TickRate.ToString() );
		}
		// Menu.
		else
		{
			OtherData.Add( "realm", "menu" );
		}

		if ( Game.IsDedicatedServer )
			return;

		OtherData.Add( "isEditorRunning", Game.IsEditor.ToString() );
		OtherData.Add( "isHandheld", Game.IsRunningOnHandheld.ToString() );
		OtherData.Add( "isVr", Game.IsRunningInVR.ToString() );

		StartTime = Stopwatch.GetElapsedTime( 0 );
	}
}
