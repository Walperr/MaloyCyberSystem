using System.Text.Json.Serialization;

namespace MQTTServer.Dto;

public sealed class TimeBounds
{
    [JsonPropertyName("beginTime")]
    public DateTime BeginTime { get; set; }
    
    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }
}