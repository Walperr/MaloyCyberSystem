using System.Windows.Input;
using ReactiveUI;

namespace MaloyClient.ViewModels.Implementation;

internal sealed class LoginViewModel : ViewModelBase, ILoginViewModel
{
    private ICommand? _disconnectCommand;
    private string? _error;
    private bool _isConnected;
    private ICommand? _loginCommand;
    private string _password = string.Empty;
    private ICommand? _registerCommand;
    private string _repeatPassword = string.Empty;
    private string _username = string.Empty;

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
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            Error = "Username and password cannot be empty";
        }
        else
        {
            Error = null;
            IsConnected = true;
        }
    });

    public ICommand RegisterCommand => _registerCommand ??= ReactiveCommand.Create(() =>
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            Error = "Username and password cannot be empty";
        }
        else if (Password != RepeatPassword)
        {
            Error = "Password are not equal";
        }
        else
        {
            Error = null;
            IsConnected = true;
        }
    });

    public ICommand DisconnectCommand => _disconnectCommand ??= ReactiveCommand.Create(() => IsConnected = false);
}