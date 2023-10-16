using System;
using System.Text.Json.Serialization;

namespace MaloyClient.Dto;

public sealed class DeviceValues
{
    [JsonPropertyName("times")]
    public required ulong[] Times { get; set; }
    
    [JsonPropertyName("values")]
    public required double[] Values { get; set; }
    
    public static DeviceValues Empty = new() { Times = Array.Empty<ulong>(), Values = Array.Empty<double>() };
}