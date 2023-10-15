using System;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Plot.Misc;
using Avalonia.Plot.ViewModel;
using MaloyClient.Models;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace MaloyClient.ViewModels.Implementation;

internal sealed class DeviceTabViewModel : ViewModelBase, IDeviceTabViewModel
{
    private ICommand? _refreshData;
    private readonly IClientService _clientService;
    private readonly IPlotSeries _series;

    public DeviceTabViewModel(IDevice device, IServiceProvider services)
    {
        _clientService = services.GetRequiredService<IClientService>();
        
        Device = device;

        Plot = PlotFactory.CreatePlot();

        var mainColor = (Color) (Application.Current?.FindResource("BackgroundHighlightColor") ?? Colors.White);
        var linesColor = (Color) (Application.Current?.FindResource("BackgroundSelectedColor") ?? Colors.DimGray);

        Plot.Background = mainColor;
        Plot.GridLinesColor = linesColor;
        Plot.AutoScale = true;

        _series = PlotFactory.CreateSeries();
        
        _series.Color = Colors.Orange;
        _series.Thickness = 2;

        Plot.AddSeries(_series);
    }

    public IDevice Device { get; }
    public IPlotViewModel Plot { get; }
    public ICommand RefreshDataCommand => _refreshData ??= ReactiveCommand.CreateFromTask(async () =>
    {
        var values = await _clientService.GetDeviceData(Device.SerialNumber, Device.TimeMin, Device.TimeMax);

        _series.XValues = values.Times.Select((t, i) =>
        {
            // var year = t.Year;
            // var day = t.DayOfYear;
            // var hour = t.Hour;
            // var minute = t.Minute;
            // var sec = t.Second;
            //
            // return (double) sec + minute * 60 + hour * 60 * 60 + day * 24 * 60 * 60 + year * 365 * 24 * 60 * 60;
            return (double)i;
        }).OrderBy(x => x).ToArray();

        _series.YValues = values.Values;
    });
}