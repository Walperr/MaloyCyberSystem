using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Plot.Misc;
using Avalonia.Plot.ViewModel;
using MaloyClient.Misc;
using MaloyClient.Models;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace MaloyClient.ViewModels.Implementation;

internal sealed class DeviceTabViewModel : ViewModelBase, IDeviceTabViewModel
{
    private ICommand? _refreshData;
    private readonly IClientService _clientService;
    private readonly IPlotSeries _series;
    private readonly ObservableCollection<DataRow> _deviceData = new();

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
        _deviceData.Clear();
        
        var values = await _clientService.GetDeviceData(Device.SerialNumber, Device.TimeMin, Device.TimeMax);

        var newData = values.Times
            .Select((t, i) =>
                new DataRow(i, t.UnixTimeStampToDateTime(), values.Values[i]));
        
        foreach (var dataRow in newData)
            _deviceData.Add(dataRow);

        _series.XValues = values.Times.Select(l => (double)l).ToArray();
        _series.YValues = values.Values;
    });

    public IEnumerable<DataRow> DeviceData => _deviceData;
}