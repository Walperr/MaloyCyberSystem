using Avalonia.Media;
using Avalonia.Plot.Misc;
using ReactiveUI;

namespace Avalonia.Plot.ViewModel.Implementation;

internal sealed class PlotSeries : ReactiveObject, IPlotSeries 
{
    private Color _color;
    private string? _name;
    private LineStyle _lineStyle;
    private int _thickness = 1;
    private double[] _xValues = Array.Empty<double>();
    private double[] _yValues = Array.Empty<double>();

    public Color Color
    {
        get => _color;
        set
        {
            if (_color == value)
                return;
            
            _color = value;
            
            this.RaisePropertyChanged();
        }
    }

    public string? Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;
            
            _name = value;
            
            this.RaisePropertyChanged();
        }
    }

    public LineStyle LineStyle
    {
        get => _lineStyle;
        set
        {
            if (_lineStyle == value)
                return;
            
            _lineStyle = value;
            
            this.RaisePropertyChanged();
        }
    }

    public int Thickness
    {
        get => _thickness;
        set
        {
            if (_thickness == value)
                return;
            
            _thickness = value;
            
            this.RaisePropertyChanged();
        }
    }

    public double[] XValues
    {
        get => _xValues;
        set
        {
            _xValues = value;
            
            this.RaisePropertyChanged();
        }
    }

    public double[] YValues
    {
        get => _yValues;
        set
        {
            _yValues = value;

            this.RaisePropertyChanged();
        }
    }
}