using System.ComponentModel.DataAnnotations;

namespace MQTTServer.Models;

public sealed class Device
{
    [Key]
    [Required]
    public required string DeviceID { get; set; }
    
    [Required]
    public required string DeviceName { get; set; }
    
    [Required]
    public required string PasswordHash { get; set; }
    
    [Required]
    public required string Salt { get; set; }
    
    public List<string>? SubscribedTopics { get; set; }
    
    public ulong[]? Times { get; set; }
    
    public double[]? Values { get; set; }

    public int ConnectionCode { get; set; } = -1;
    
    public void AddValue(ulong time, double value)
    {
        var times = Times ??= Array.Empty<ulong>();
        var values = Values ??= Array.Empty<double>();
        
        Array.Resize(ref times, times.Length + 1);
        Array.Resize(ref values, values.Length + 1);

        times[^1] = time;
        values[^1] = value;

        Times = times;
        Values = values;
    }

    public void Clear()
    {
        Times = Array.Empty<ulong>();
        Values = Array.Empty<double>();
    }
}