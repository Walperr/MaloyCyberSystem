using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using MaloyClient.Models;
using MaloyClient.Models.Implementation;
using ReactiveUI;

namespace MaloyClient.ViewModels.Implementation;

internal sealed class MainViewModel : ViewModelBase, IMainViewModel
{
    private readonly ObservableCollection<IDevice> _allDevices = new();
    private readonly IClientService _clientService;
    private readonly ObservableCollection<Command> _commands = new();
    private readonly ObservableCollection<IDevice> _connectedDevices = new();
    private readonly ObservableCollection<IDeviceTabViewModel> _deviceTabs = new();
    private readonly ILoginViewModel _loginViewModel;
    private readonly ObservableCollection<Notification> _notifications = new();
    private readonly IServiceProvider _services;
    private ICommand? _addNewCommand;
    private ICommand? _cancelConnectionCommand;
    private ICommand? _confirmConnectionCommand;
    private ICommand? _connectDeviceCommand;
    private int _deviceToConnectIndex;
    private ICommand? _executeCommand;
    private Notification? _lastNotification;
    private ICommand? _openDeviceTabCommand;
    private IDevice? _selectedDevice;
    private IDeviceTabViewModel? _selectedTab;
    private bool _showCommands;
    private bool _showConfirmation;

    public MainViewModel(ILoginViewModel loginViewModel, IClientService clientService, IServiceProvider services)
    {
        _loginViewModel = loginViewModel;
        _clientService = clientService;
        _services = services;

        _clientService.Connected += ClientOnConnected;
        _clientService.MessageReceived += OnMessageReceived;
        _clientService.DevicesChanged += ClientOnDevicesChanged;
        _clientService.ConnectedDevicesChanged += ClientOnConnectedDevicesChanged;
        _clientService.Disconnected += ClientOnDisconnected;

        _notifications.CollectionChanged += NotificationsOnCollectionChanged;
    }

    public Notification? LastNotification
    {
        get => _lastNotification;
        set => this.RaiseAndSetIfChanged(ref _lastNotification, value);
    }

    public string Username => _loginViewModel.Username;
    public string Password => _loginViewModel.Password;

    public bool ShowCommands
    {
        get => _showCommands;
        set => this.RaiseAndSetIfChanged(ref _showCommands, value);
    }

    public bool ShowConfirmation
    {
        get => _showConfirmation;
        set => this.RaiseAndSetIfChanged(ref _showConfirmation, value);
    }

    public IEnumerable<IDevice> ConnectedDevices => _connectedDevices;

    public IEnumerable<IDevice> AllDevices => _allDevices;

    public IEnumerable<Notification> Notifications => _notifications;

    public IEnumerable<Command> Commands => _commands;

    public IEnumerable<IDeviceTabViewModel> DeviceTabs => _deviceTabs;

    public IDevice? SelectedDevice
    {
        get => _selectedDevice;
        set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
    }

    public int DeviceToConnectIndex
    {
        get => _deviceToConnectIndex;
        set => this.RaiseAndSetIfChanged(ref _deviceToConnectIndex, value);
    }

    public IDeviceTabViewModel? SelectedTab
    {
        get => _selectedTab;
        set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
    }

    public ICommand ConnectDeviceCommand => _connectDeviceCommand ??= ReactiveCommand.Create(() =>
    {
        if (DeviceToConnectIndex == -1)
            return;

        var device = _allDevices[DeviceToConnectIndex];
        
        if (ConnectedDevices.Contains(device))
            return;

        _clientService.ConnectDevice(device.SerialNumber);

        ShowConfirmation = true;
    });

    public ICommand ConfirmConnectionCommand => _confirmConnectionCommand ??= ReactiveCommand.Create<string>(code =>
    {
        if (DeviceToConnectIndex == -1)
            return;

        var device = _allDevices[DeviceToConnectIndex];

        _clientService.ConfirmConnection(device.SerialNumber, int.Parse(code));

        ShowConfirmation = false;
    });

    public ICommand AddNewCommand => _addNewCommand ??= ReactiveCommand.Create(() =>
    {
        _commands.Add(new Command("New command", "action"));
    });

    public ICommand ExecuteCommand => _executeCommand ??= ReactiveCommand.Create<Command>(command =>
    {
        if (SelectedDevice is null)
            return;

        _clientService.SendCommand(SelectedDevice.SerialNumber, command);
    });

    public ICommand DisconnectCommand => _loginViewModel.DisconnectCommand;

    public ICommand OpenDeviceTabCommand => _openDeviceTabCommand ??= ReactiveCommand.Create<IDevice>(device =>
    {
        var tab = _deviceTabs.FirstOrDefault(t => t.Device == device);

        if (tab is null)
        {
            tab = new DeviceTabViewModel(device, _services);
            _deviceTabs.Add(tab);
        }

        SelectedTab = tab;
    });

    public ICommand CancelConnectionCommand => _cancelConnectionCommand ??= ReactiveCommand.Create(() =>
    {
        ShowConfirmation = false;

        var device = _allDevices[DeviceToConnectIndex];

        _clientService.CancelDeviceConnection(device.SerialNumber);
    });

    private void ClientOnDisconnected(object? sender, EventArgs e)
    {
        _deviceTabs.Clear();
        _allDevices.Clear();
        _connectedDevices.Clear();
    }

    private async void ClientOnConnectedDevicesChanged(object? sender, EventArgs e)
    {
        var newDevices = (await _clientService.GetConnectedDevices()).ToArray();
        var existingDevices = _connectedDevices;

        var leftJoin = from device in newDevices
            join existingDevice in existingDevices
                on device.DeviceID equals existingDevice.SerialNumber
                into tmp
            select (device, deviceViewModel: tmp.DefaultIfEmpty().FirstOrDefault());

        var rightJoin = from existingDevice in existingDevices
            join newDevice in newDevices
                on existingDevice.SerialNumber equals newDevice.DeviceID
                into tmp
            select (device: tmp.DefaultIfEmpty().FirstOrDefault(), deviceViewModel: existingDevice);

        var fullJoin = leftJoin.Union(rightJoin).ToArray();

        foreach (var (device, deviceViewModel) in fullJoin)
            switch (device, deviceViewModel)
            {
                case (null, null):
                    break;
                case (null, not null):
                    _connectedDevices.Remove(deviceViewModel);
                    continue;
                case (not null, null):
                    _connectedDevices.Add(_allDevices.FirstOrDefault(d => d.SerialNumber == device.DeviceID)!);
                    continue;
                case (not null, not null):
                    deviceViewModel.Name = device.Name;
                    break;
            }
    }

    private async void ClientOnDevicesChanged(object? sender, EventArgs e)
    {
        var newDevices = (await _clientService.GetAllDevices()).ToArray();
        var existingDevices = _allDevices;

        var leftJoin = from device in newDevices
            join existingDevice in existingDevices
                on device.DeviceID equals existingDevice.SerialNumber
                into tmp
            select (device, deviceViewModel: tmp.DefaultIfEmpty().FirstOrDefault());

        var rightJoin = from existingDevice in existingDevices
            join newDevice in newDevices
                on existingDevice.SerialNumber equals newDevice.DeviceID
                into tmp
            select (device: tmp.DefaultIfEmpty().FirstOrDefault(), deviceViewModel: existingDevice);

        var fullJoin = leftJoin.Union(rightJoin).ToArray();

        foreach (var (device, deviceViewModel) in fullJoin)
            switch (device, deviceViewModel)
            {
                case (null, null):
                    break;
                case (null, not null):
                    _connectedDevices.Remove(deviceViewModel);
                    _allDevices.Remove(deviceViewModel);
                    continue;
                case (not null, null):
                    _allDevices.Add(new Device(device.DeviceID, device.Name, _clientService));
                    continue;
                case (not null, not null):
                    deviceViewModel.Name = device.Name;
                    break;
            }
    }

    private void OnMessageReceived(object? sender, Notification e)
    {
        _notifications.Add(e);
    }

    private async void ClientOnConnected(object? sender, EventArgs e)
    {
        _allDevices.Clear();
        _connectedDevices.Clear();

        var devices = await _clientService.GetAllDevices();
        var connectedDevices = (await _clientService.GetConnectedDevices()).ToArray();

        foreach (var device in devices)
        {
            var deviceViewModel = new Device(device.DeviceID, device.Name, _clientService);
            _allDevices.Add(deviceViewModel);
            if (connectedDevices.Any(d => d.DeviceID == device.DeviceID))
                _connectedDevices.Add(deviceViewModel);
        }
    }

    private void NotificationsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        LastNotification = _notifications.LastOrDefault();
    }
}