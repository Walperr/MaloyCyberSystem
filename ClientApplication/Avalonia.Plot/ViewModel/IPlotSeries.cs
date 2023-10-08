using System.ComponentModel;
using Avalonia.Media;
using Avalonia.Plot.Misc;

namespace Avalonia.Plot.ViewModel;

public interface IPlotSeries : INotifyPropertyChanged
{
    Color Color { get; set; }
    string? Name { get; set; }
    LineStyle LineStyle { get; set; }
    int Thickness { get; set; }

    double[] XValues { get; set; }
    double[] YValues { get; set; }
}