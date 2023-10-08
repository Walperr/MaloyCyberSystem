﻿using System;
using System.Linq;
using Avalonia.Media;
using Avalonia.Plot.Misc;
using Avalonia.Plot.ViewModel;

namespace MaloyClient.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public IPlotViewModel Plot { get; }

    public MainWindowViewModel()
    {
        Plot = PlotFactory.CreatePlot();

        var xMin = -1;
        var xStep = 0.0004;

        var x = Enumerable.Range(0, 5000).Select(i => xMin + xStep * i).ToArray();
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
        
        series.Color = Colors.Blue;
        series.LineStyle = LineStyle.Dash;
        series.Thickness = 2;
        
        Plot.AddSeries(series);
        
        y = x.Select(d => Math.Cos(d * 3) * 1.5).ToArray();
        
        series = PlotFactory.CreateSeries();

        series.XValues = x;
        series.YValues = y;
        
        series.Color = Colors.Green;
        series.LineStyle = LineStyle.DashDotDot;
        series.Thickness = 2;
        
        Plot.AddSeries(series);
        Plot.AutoScale = false;
    }
    
    
}