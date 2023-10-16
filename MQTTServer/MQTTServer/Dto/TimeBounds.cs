using System.Text.Json.Serialization;

namespace MQTTServer.Dto;

public sealed class TimeBounds
{
    [JsonPropertyName("beginTime")]
    public ulong BeginTime { get; set; }
    
    [JsonPropertyName("endTime")]
    public ulong EndTime { get; set; }
}