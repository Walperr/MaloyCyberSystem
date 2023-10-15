using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using DynamicData;
using MaloyClient.Models;
using MaloyClient.Models.Implementation;
using ReactiveUI;

namespace MaloyClient.ViewModels.Implementation;

internal sealed class MainViewModel : ViewModelBase, IMainViewModel
{
    private readonly ObservableCollection<IDevice> _allDevices = new();
    private readonly ObservableCollection<Command> _commands = new();
    private readonly ObservableCollection<IDevice> _connectedDevices = new();
    private readonly ObservableCollection<IDeviceTabViewModel> _deviceTabs = new();
    private readonly ILoginViewModel _loginViewModel;
    private readonly ObservableCollection<Notification> _notifications = new();
    private ICommand? _addNewCommand;
    private ICommand? _connectDeviceCommand;
    private int _deviceToConnectIndex;
    private ICommand? _executeCommand;
    private IDevice? _selectedDevice;
    private IDeviceTabViewModel? _selectedTab;
    private ICommand? _openDeviceTabCommand;
    private Notification? _lastNotification;
    private bool _showCommands;

    public MainViewModel(ILoginViewModel loginViewModel)
    {
        _loginViewModel = loginViewModel;

        //todo: remove this

        var devices = new IDevice[]
        {
            new Device("SN001", "Device 1"),
            new Device("SN002", "Device 2"),
            new Device("SN003", "Device 3"),
            new Device("SN004", "Device 4"),
            new Device("SN005", "Device 5"),
            new Device("SN006", "Device 6"),
            new Device("SN007", "Device 7"),
            new Device("SN008", "Device 8"),
            new Device("SN009", "Device 9"),
            new Device("SN010", "Device 10")
        };

        _allDevices.AddRange(devices);

        _commands.Add(new Command("Turn on rele", "invoke rele", false));
        _commands.Add(new Command("Restart", "restart", false));
        _commands.Add(new Command("Print value", "print value", false));
        
        _notifications.CollectionChanged += NotificationsOnCollectionChanged;

        _notifications.Add(new Notification("SN001", "Started"));
        _notifications.Add(new Notification("SN002", "Started"));
        _notifications.Add(new Notification("SN004", "Started"));
        _notifications.Add(new Notification("SN005", "Started"));
        _notifications.Add(new Notification("SN005", "Message 1"));
        _notifications.Add(new Notification("SN002", "Hello, world!"));
    }

    private void NotificationsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        LastNotification = _notifications.LastOrDefault();
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

    public int DeviceToConnectIndex    {
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
        
        if (!_connectedDevices.Contains(device)) 
            _connectedDevices.Add(device);
    });

    public ICommand AddNewCommand => _addNewCommand ??= ReactiveCommand.Create(() =>
    {
        _commands.Add(new Command("New command", "action"));
    });

    public ICommand ExecuteCommand => _executeCommand ??= ReactiveCommand.Create<Command>(command =>
    {
        if (SelectedDevice is null)
            return;

        _notifications.Add(new Notification(SelectedDevice.SerialNumber, $"Invoked command:\n\r {command.CommandName}"));
    });

    public ICommand DisconnectCommand => _loginViewModel.DisconnectCommand;

    public ICommand OpenDeviceTabCommand => _openDeviceTabCommand ??= ReactiveCommand.Create<IDevice>(device =>
    {
        var tab = _deviceTabs.FirstOrDefault(t => t.Device == device);

        if (tab is null)
        {
            tab = new DeviceTabViewModel(device);
            _deviceTabs.Add(tab);
        }
        
        SelectedTab = tab;
    });
}