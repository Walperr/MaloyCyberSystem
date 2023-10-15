using System.Text.Json.Serialization;

namespace MaloyClient.Dto;

public sealed class Device
{
    [JsonPropertyName("deviceID")]
    public required string DeviceID { get; set; }
    
    [JsonPropertyName("deviceName")]
    public required string Name { get; set; }
}