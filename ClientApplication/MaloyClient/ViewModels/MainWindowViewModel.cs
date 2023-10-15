using MaloyClient.Models;
using MaloyClient.Models.Implementation;
using MaloyClient.ViewModels.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace MaloyClient.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public IMainViewModel MainViewModel { get; }
    public ILoginViewModel LoginViewModel { get; }

    public MainWindowViewModel()
    {
        IServiceCollection container = new ServiceCollection();

        container.AddSingleton<IMainViewModel, MainViewModel>();
        container.AddSingleton<ILoginViewModel, LoginViewModel>();
        container.AddSingleton<IClientService, ClientService>();

        var services = container.BuildServiceProvider();
        
        LoginViewModel = services.GetRequiredService<ILoginViewModel>();
        MainViewModel = services.GetRequiredService<IMainViewModel>();
    }
}