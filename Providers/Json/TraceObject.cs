using Sandbox;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace PerformanceTracing.Providers.Json;

internal sealed class TraceObject
{
	[JsonPropertyName( "traceEvents" )]
	public ConcurrentBag<TraceEvent> TraceEvents { get; set; } = new();
	[JsonPropertyName( "otherData" )]
	public Dictionary<string, object?> MetaData { get; init; } = new()
	{
		{ "perfTracingVersion", "1.0.0" },
	};
	internal TimeSpan StartTime { get; }

	public TraceObject()
	{
		if ( Game.IsServer )
		{
			MetaData.Add( "realm", "server" );
			MetaData.Add( "serverSteamid", Game.ServerSteamId );
			MetaData.Add( "isDedicatedServer", Game.IsDedicatedServer );
			MetaData.Add( "tickRate", Game.TickRate );
		}
		else if ( Game.IsClient )
		{
			MetaData.Add( "realm", "client" );
			MetaData.Add( "steamid", Game.SteamId );
			MetaData.Add( "isServerHost", Game.IsServerHost );
			MetaData.Add( "tickRate", Game.TickRate );
		}
		// Menu.
		else
		{
			MetaData.Add( "realm", "menu" );
		}

		if ( Game.IsDedicatedServer )
			return;

		MetaData.Add( "isEditorRunning", Game.IsEditor );
		MetaData.Add( "isHandheld", Game.IsRunningOnHandheld );
		MetaData.Add( "isVr", Game.IsRunningInVR );

		StartTime = Stopwatch.GetElapsedTime( 0 );
	}
}
