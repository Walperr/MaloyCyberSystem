using System;
using System.Collections.Generic;
using MaloyClient.Dto;

namespace MaloyClient.Models;

public interface IClientService
{
    string ServerIP { get; set; }
    int ServerPort { get; set; }

    string? Error { get; }

    bool TryConnect(string userID, string password, bool register = false);
    void Disconnect();

    event EventHandler<Notification> MessageReceived;
    event EventHandler Connected;
    event EventHandler Disconnected;
    event EventHandler ConnectedDevicesChanged;
    event EventHandler DevicesChanged;

    IEnumerable<Device> GetAllDevices();
    IEnumerable<Device> GetConnectedDevices();

    void ConnectDevice(string deviceID);
    void ConfirmConnection(string deviceID, int code);

    void SendCommand(string deviceID, Command command);

    DeviceValues GetDeviceData(string deviceID, DateTime start, DateTime end);

    void RenameDevice(string deviceID, string name);
}