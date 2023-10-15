using System.Text.Json.Serialization;

namespace MQTTServer.Dto;

public sealed class DeviceValue
{
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }
    
    [JsonPropertyName("value")]
    public double Value { get; set; }
}