using System;

namespace MaloyClient.Dto;

public sealed class DeviceValues
{
    public required DateTime[] Times { get; set; }
    public required double[] Values { get; set; }
}