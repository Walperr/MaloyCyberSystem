using System;
using ReactiveUI;

namespace MaloyClient.Models.Implementation;

internal sealed class Device : ReactiveObject, IDevice
{
    private string _name;
    private readonly IClientService _clientService;
    private DateTime _timeMax = DateTime.Now;
    private DateTime _timeMin = DateTime.Today;

    public Device(string serialNumber, string name, IClientService clientService)
    {
        SerialNumber = serialNumber;
        _name = name;
        _clientService = clientService;
    }

    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            _clientService.RenameDevice(SerialNumber, value);
        }
    }

    public string SerialNumber { get; }

    public DateTime TimeMin
    {
        get => _timeMin;
        set => this.RaiseAndSetIfChanged(ref _timeMin, value);
    }

    public DateTime TimeMax
    {
        get => _timeMax;
        set => this.RaiseAndSetIfChanged(ref _timeMax, value);
    }

    public DateTime[] Times { get; } = Array.Empty<DateTime>();
    public double[] Values { get; } = Array.Empty<double>();

    public bool Online { get; } = true;
}