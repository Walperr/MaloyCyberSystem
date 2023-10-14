using System.Windows.Input;

namespace MaloyClient.ViewModels;

public interface ILoginViewModel
{
    string Username { get; set; }
    string Password { get; set; }
    string RepeatPassword { get; set; }
    
    string? Error { get; } 
    bool IsConnected { get; }
    
    ICommand LoginCommand { get; }
    ICommand RegisterCommand { get; }
    
    ICommand DisconnectCommand { get; }
}