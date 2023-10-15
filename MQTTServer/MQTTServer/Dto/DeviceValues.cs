using System.Text.Json.Serialization;

namespace MQTTServer.Dto;

public sealed class DeviceValues
{
    [JsonPropertyName("times")]
    public required DateTime[] Times { get; set; }
    
    [JsonPropertyName("values")]
    public required double[] Values { get; set; }

    public static DeviceValues Empty = new() {Times = Array.Empty<DateTime>(), Values = Array.Empty<double>()};
}