using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaloyClient.Dto;

namespace MaloyClient.Models;

public interface IClientService
{
    string ServerIP { get; set; }
    int ServerPort { get; set; }

    string? Error { get; }

    Task<bool> TryConnect(string userID, string password, bool register = false);
    void Disconnect();

    event EventHandler<Notification> MessageReceived;
    event EventHandler Connected;
    event EventHandler Disconnected;
    event EventHandler ConnectedDevicesChanged;
    event EventHandler DevicesChanged;

    Task<IEnumerable<Device>> GetAllDevices();
    Task<IEnumerable<Device>> GetConnectedDevices();

    void ConnectDevice(string deviceID);
    void ConfirmConnection(string deviceID, int code);

    void SendCommand(string deviceID, Command command);

    Task<DeviceValues> GetDeviceData(string deviceID, DateTime start, DateTime end);

    void RenameDevice(string deviceID, string name);
    Task CancelDeviceConnection(string deviceID);
}