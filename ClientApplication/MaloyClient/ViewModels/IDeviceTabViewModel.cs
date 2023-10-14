using System.Windows.Input;
using Avalonia.Plot.ViewModel;
using MaloyClient.Models;

namespace MaloyClient.ViewModels;

public interface IDeviceTabViewModel
{
    IDevice Device { get; }
    
    IPlotViewModel Plot { get; }
    
    ICommand RefreshDataCommand { get; }
}