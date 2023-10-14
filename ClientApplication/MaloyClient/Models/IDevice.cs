using System;

namespace MaloyClient.Models;

public interface IDevice
{
    string Name { get; set; }
    string SerialNumber { get; }
    
    DateTime TimeMin { get; set; }
    DateTime TimeMax { get; set; }
    
    DateTime[] Times { get; }
    double[] Values { get; }
    
    bool Online { get; }
}