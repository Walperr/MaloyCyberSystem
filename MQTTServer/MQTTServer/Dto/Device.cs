using System.Text.Json.Serialization;

namespace MQTTServer.Dto;

public sealed class Device
{
    [JsonPropertyName("deviceName")]
    public required string DeviceName { get; set; }
    
    [JsonPropertyName("deviceID")]
    public required string DeviceID { get; set; }
}