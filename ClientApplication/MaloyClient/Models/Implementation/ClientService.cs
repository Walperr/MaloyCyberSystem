using System;
using System.Collections.Generic;
using MaloyClient.Dto;

namespace MaloyClient.Models.Implementation;

internal sealed class ClientService : IClientService, IDisposable
{
    public string ServerIP { get; set; }
    public int ServerPort { get; set; }
    public string? Error { get; private set; }

    public bool TryConnect(string userID, string password, bool register = false)
    {
        var next = Random.Shared.Next(100);

        if (next < 50)
        {
            Error = "Connection failed";
            return false;
        }

        return true;
    }

    public void Disconnect()
    {
    }

    public event EventHandler<Notification>? MessageReceived;
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;
    public event EventHandler? ConnectedDevicesChanged;
    public event EventHandler? DevicesChanged;

    public IEnumerable<Dto.Device> GetAllDevices()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Dto.Device> GetConnectedDevices()
    {
        throw new NotImplementedException();
    }

    public void ConnectDevice(string deviceID)
    {
        throw new NotImplementedException();
    }

    public void ConfirmConnection(string deviceID, int code)
    {
        throw new NotImplementedException();
    }

    public void SendCommand(string deviceID, Command command)
    {
        throw new NotImplementedException();
    }

    public DeviceValues GetDeviceData(string deviceID, DateTime start, DateTime end)
    {
        throw new NotImplementedException();
    }

    public void RenameDevice(string deviceID, string name)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}