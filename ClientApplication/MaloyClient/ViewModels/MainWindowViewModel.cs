using System;
using System.Linq;
using Avalonia.Media;
using Avalonia.Media.Fonts;
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
        var xStep = 0.1;

        var x = Enumerable.Range(0, 100000).Select(i => xMin + xStep * i).ToArray();
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
        // series.LineStyle = LineStyle.Dash;
        series.Thickness = 2;
        
        Plot.AddSeries(series);
        
       y = x.Select(d => Math.Cos(d * 3) * 1.5).ToArray();
        
        series = PlotFactory.CreateSeries();

        series.XValues = x;
        series.YValues = y;
        
        series.Color = Colors.Green;
        // series.LineStyle = LineStyle.DashDotDot;
        series.Thickness = 2;
        
        Plot.AddSeries(series);
        
        y = x.Select(d => Math.Cos(d) * 0.5).ToArray();
        
        series = PlotFactory.CreateSeries();

        series.XValues = x;
        series.YValues = y;
        
        series.Color = Colors.Brown;
        // series.LineStyle = LineStyle.DashDotDot;
        series.Thickness = 3;
        
        Plot.AddSeries(series);
       
        Plot.AutoScale = false;

        var fonts = FontManager.Current.SystemFonts;

        foreach (var font in fonts)
        {
            Console.WriteLine(font.Name);
        }
    }
    
    
}