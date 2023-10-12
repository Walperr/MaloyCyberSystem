using System.ComponentModel;
using Avalonia.Media;

namespace Avalonia.Plot.ViewModel;

public interface IPlotViewModel : INotifyPropertyChanged
{
    Color GridLinesColor { get; set; }
    Color Background { get; set; }
    IEnumerable<IPlotSeries> Series { get; }
    void AddSeries(IPlotSeries series);
    void RemoveSeries(IPlotSeries series);
    
    double XMin { get; set; }
    double XMax { get; set; }
    
    double XAxisStep { get; set; }
    
    double YMin { get; set; }
    double YMax { get; set; }
    
    double YAxisStep { get; set; }
    
    bool AutoScale { get; set; }

    internal event EventHandler RedrawSuggested;
}