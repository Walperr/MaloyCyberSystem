using System;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Plot.Misc;
using Avalonia.Plot.ViewModel;
using MaloyClient.Models;
using ReactiveUI;

namespace MaloyClient.ViewModels.Implementation;

internal sealed class DeviceTabViewModel : ViewModelBase, IDeviceTabViewModel
{
    private ICommand? _refreshData;

    public DeviceTabViewModel(IDevice device)
    {
        Device = device;

        Plot = PlotFactory.CreatePlot();

        var mainColor = (Color) (Application.Current?.FindResource("BackgroundHighlightColor") ?? Colors.White);
        var linesColor = (Color) (Application.Current?.FindResource("BackgroundSelectedColor") ?? Colors.DimGray);

        Plot.Background = mainColor;
        Plot.GridLinesColor = linesColor;

        var xMin = -Math.PI;
        var xStep = Math.PI * 2 / 1000000;

        var x = Enumerable.Range(0, 1000000).Select(i => xMin + xStep * i).ToArray();
        var y = x.Select(Math.Sin).ToArray();

        var series = PlotFactory.CreateSeries();

        series.XValues = x;
        series.YValues = y;

        series.Color = Colors.Red;
        series.LineStyle = LineStyle.Solid;

        Plot.AddSeries(series);

        y = x.Select(d => Math.Sin(d * 3) * 2).ToArray();

        series = PlotFactory.CreateSeries();

        series.XValues = x;
        series.YValues = y;

        series.Color = Colors.Orange;
        series.Thickness = 2;

        Plot.AddSeries(series);

        y = x.Select(d => Math.Cos(d * 3) * 1.5).ToArray();

        series = PlotFactory.CreateSeries();

        series.XValues = x;
        series.YValues = y;

        series.Color = Colors.CadetBlue;
        series.Thickness = 2;

        Plot.AddSeries(series);

        Plot.AutoScale = false;
    }

    public IDevice Device { get; }
    public IPlotViewModel Plot { get; }
    public ICommand RefreshDataCommand => _refreshData ??= ReactiveCommand.Create(() => { });
}