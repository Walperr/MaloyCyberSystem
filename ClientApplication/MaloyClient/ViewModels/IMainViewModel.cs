using System.Collections.Generic;
using System.Windows.Input;
using MaloyClient.Models;

namespace MaloyClient.ViewModels;

public interface IMainViewModel
{ 
    string Username { get; }
    string Password { get; }
    
    bool ShowCommands { get; set; }
    
    IEnumerable<IDevice> ConnectedDevices { get; }
    IEnumerable<IDevice> AllDevices { get; }
    
    IEnumerable<Notification> Notifications { get; }
    
    IEnumerable<Command> Commands { get; }
    
    IEnumerable<IDeviceTabViewModel> DeviceTabs { get; }
    
    IDevice? SelectedDevice { get; set; }
    IDeviceTabViewModel? SelectedTab { get; set; }
    
    int DeviceToConnectIndex { get; set; }

    ICommand ConnectDeviceCommand { get; }
    ICommand ConfirmConnectionCommand { get; }
    ICommand AddNewCommand { get; }
    ICommand ExecuteCommand { get; }
    ICommand DisconnectCommand { get; }
    
    ICommand OpenDeviceTabCommand { get; }
}