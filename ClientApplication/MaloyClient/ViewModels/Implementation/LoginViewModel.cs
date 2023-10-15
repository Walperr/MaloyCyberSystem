using System;
using System.Windows.Input;
using MaloyClient.Models;
using ReactiveUI;

namespace MaloyClient.ViewModels.Implementation;

internal sealed class LoginViewModel : ViewModelBase, ILoginViewModel
{
    private readonly IClientService _clientService;
    private ICommand? _disconnectCommand;
    private string? _error;
    private bool _isConnected;
    private ICommand? _loginCommand;
    private string _password = string.Empty;
    private int _port = 1883;
    private ICommand? _registerCommand;
    private string _repeatPassword = string.Empty;
    private string _serverIp = "localhost";
    private string _username = string.Empty;

    public LoginViewModel(IClientService clientService)
    {
        _clientService = clientService;
        _clientService.Disconnected += ClientServiceOnDisconnected;
    }

    private void ClientServiceOnDisconnected(object? sender, EventArgs e)
    {
        IsConnected = false;
        Error = _clientService.Error;
    }

    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public string RepeatPassword
    {
        get => _repeatPassword;
        set => this.RaiseAndSetIfChanged(ref _repeatPassword, value);
    }

    public string ServerIP
    {
        get => _serverIp;
        set
        {
            this.RaiseAndSetIfChanged(ref _serverIp, value);
            _clientService.ServerIP = value;
        }
    }

    public int Port
    {
        get => _port;
        set
        {
            this.RaiseAndSetIfChanged(ref _port, value);
            _clientService.ServerPort = value;
        }
    }

    public string? Error
    {
        get => _error;
        private set => this.RaiseAndSetIfChanged(ref _error, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set => this.RaiseAndSetIfChanged(ref _isConnected, value);
    }

    public ICommand LoginCommand => _loginCommand ??= ReactiveCommand.Create(() =>
    {
        Error = null;
        if (string.IsNullOrEmpty(ServerIP))
        {
            Error = "Address cannot be empty";
        }
        else if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            Error = "Username and password cannot be empty";
        }
        else
        {
            var isConnected = _clientService.TryConnect(Username, Password);

            Error = _clientService.Error;
            IsConnected = isConnected;
        }
    });

    public ICommand RegisterCommand => _registerCommand ??= ReactiveCommand.Create(() =>
    {
        Error = null;
        if (string.IsNullOrEmpty(ServerIP))
        {
            Error = "Address cannot be empty";
        }
        else if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            Error = "Username and password cannot be empty";
        }
        else if (Password != RepeatPassword)
        {
            Error = "Passwords are not equal";
        }
        else
        {
            var isConnected = _clientService.TryConnect(Username, Password, true);

            Error = _clientService.Error;
            IsConnected = isConnected;
        }
    });

    public ICommand DisconnectCommand => _disconnectCommand ??= ReactiveCommand.Create(() => _clientService.Disconnect());
}