using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MaloyClient.Dto;
using MaloyClient.Misc;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Packets;

namespace MaloyClient.Models.Implementation;

internal sealed class ClientService : IClientService, IDisposable
{
    private IMqttClient? _mqttClient;
    private MqttFactory? _mqttFactory;
    private HttpClient? _httpClient;
    private string? _error;
    public string ServerIP { get; set; } = "localhost";
    public int ServerPort { get; set; } = 1883;

    public string? Error
    {
        get => _error;
        private set
        {
            if (_error == value)
                return;
            
            _error = value;

            if (_error is not null)
                //todo: introduce logger which can show errors 
                MessageReceived?.Invoke(this, new Notification("System", _error));
        }
    }

    public async Task<bool> TryConnect(string userID, string password, bool register = false)
    {
        Error = null;
        _mqttFactory = new MqttFactory();

        _mqttClient = _mqttFactory.CreateMqttClient();

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer($"{ServerIP}", ServerPort)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithUserProperty("User", "True")
            .WithUserProperty("Register", register.ToString())
            .WithCredentials(userID, password)
            .WithClientId(userID)
            .Build();
        
        _mqttClient.ConnectedAsync += MqttClientOnConnectedAsync;
        _mqttClient.DisconnectedAsync += MqttClientOnDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += MqttClientOnApplicationMessageReceivedAsync;

        try
        {
            var response = await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var success = response.ResultCode == MqttClientConnectResultCode.Success;

            Error = success ? null : response.ReasonString;

            _httpClient = new HttpClient {BaseAddress = new Uri($"http://{ServerIP}:{5000}")};
            
            Connected?.Invoke(this, EventArgs.Empty);

            return success;
        }
        catch (Exception e)
        {
            Error = e.Message;
            DropClient();
            return false;
        }
    }

    private void DropClient()
    {
        var mqttClient = _mqttClient;
        
        if (mqttClient != null)
        {
            mqttClient.ConnectedAsync -= MqttClientOnConnectedAsync;
            mqttClient.DisconnectedAsync -= MqttClientOnDisconnectedAsync;
            mqttClient.ApplicationMessageReceivedAsync -= MqttClientOnApplicationMessageReceivedAsync;
        }
        
        mqttClient?.Dispose();
        _mqttClient = null;
        _httpClient = null;
    }

    public static string ConvertToString(ArraySegment<byte> bytes, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;

        return new string(encoding.GetChars(bytes.ToArray()));
    }
    
    private Task MqttClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        var msg = arg.ApplicationMessage;
        
        var message = ConvertToString(msg.PayloadSegment);

        if (msg.Topic == "Server/Notifications")
        {
            if (message == "Devices updated")
                DevicesChanged?.Invoke(this, EventArgs.Empty);
            
            if (message == $"Connected device and user {_mqttClient?.Options.ClientId}")
                ConnectedDevicesChanged?.Invoke(this, EventArgs.Empty);
            
            return Task.CompletedTask;
        }

        var notification = new Notification(msg.Topic, message);

        MessageReceived?.Invoke(this, notification);
        
        return Task.CompletedTask;
    }

    private Task MqttClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        DropClient();
        Disconnected?.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }

    private async Task MqttClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        await _mqttClient.SubscribeAsync("Server/Notifications");
        
        Connected?.Invoke(this, EventArgs.Empty);

        MessageReceived?.Invoke(this, new Notification("System", $"Connected to {ServerIP}"));
    }

    public async void Disconnect()
    {
        if (_mqttClient is null)
            return;
        
        var mqttClientDisconnectOptions = _mqttFactory?.CreateClientDisconnectOptionsBuilder().Build();

        await _mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
        
        DropClient();
    }

    public event EventHandler<Notification>? MessageReceived;
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;
    public event EventHandler? ConnectedDevicesChanged;
    public event EventHandler? DevicesChanged;

    //get
    public async Task<IEnumerable<Dto.Device>> GetAllDevices()
    {
        Error = null;
        if (_httpClient is null)
            return Enumerable.Empty<Dto.Device>();

        try
        {
            var response = await _httpClient.GetAsync("api/Devices");

            return await response.Content.ReadFromJsonAsync<List<Dto.Device>>() ?? Enumerable.Empty<Dto.Device>();
        }
        catch (Exception e)
        {
            Error = e.Message;
            return Enumerable.Empty<Dto.Device>();
        }
    }
    
    //get
    public async Task<IEnumerable<Dto.Device>> GetConnectedDevices()
    {
        Error = null;
        if (_httpClient is null)
            return Enumerable.Empty<Dto.Device>();
        
        try
        {
            var response = await _httpClient.GetAsync($"api/Devices/Connected/{_mqttClient?.Options.ClientId}");

            return await response.Content.ReadFromJsonAsync<List<Dto.Device>>() ?? Enumerable.Empty<Dto.Device>();
        }
        catch (Exception e)
        {
            Error = e.Message;
            return Enumerable.Empty<Dto.Device>();
        }
    }

    public async void ConnectDevice(string deviceID)
    {
        var topic = $"Device/Connections/{deviceID}";
        
        if (_mqttClient is null)
            return;

        await _mqttClient.PublishStringAsync(topic, "connect");
    }

    public void ConfirmConnection(string deviceID, int code)
    {
        if (_mqttClient is null)
            return;

        var topic = $"Device/ConnectionKeys";

        _mqttClient.PublishAsync(new MqttApplicationMessage
        {
            Topic = topic,
            PayloadSegment = Encoding.Default.GetBytes($"{code}"),
            UserProperties = new List<MqttUserProperty> {new("User", "true"), new("deviceID", deviceID)}
        });
    }

    public async void SendCommand(string deviceID, Command command)
    {
        var topic = $"Device/Commands/{deviceID}";

        await _mqttClient.PublishStringAsync(topic, command.Content);
    }

    //get
    public async Task<DeviceValues> GetDeviceData(string deviceID, DateTime start, DateTime end)
    {
        Error = null;
        if (_httpClient is null)
            return DeviceValues.Empty;

        try
        {
            var response =
                await _httpClient.GetAsync(
                    $"api/Devices/Data/Interval/{deviceID}?beginTime={start.DateTimeToUnixTimeStamp()}&endTime={end.DateTimeToUnixTimeStamp()}");

            return await response.Content.ReadFromJsonAsync<DeviceValues>() ?? DeviceValues.Empty;
        }
        catch (Exception e)
        {
            Error = e.Message;
            return DeviceValues.Empty;
        }
    }

    //put
    public void RenameDevice(string deviceID, string name)
    {
        
    }

    public async Task CancelDeviceConnection(string deviceID)
    {
        var topic = $"Device/Connections/{deviceID}";
        
        if (_mqttClient is null)
            return;

        await _mqttClient.PublishStringAsync(topic, "cancel");
    }

    public void Dispose()
    {
        Disconnect();
    }
}