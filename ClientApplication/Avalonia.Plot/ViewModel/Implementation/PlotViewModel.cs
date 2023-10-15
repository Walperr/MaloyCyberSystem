using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Media;
using ReactiveUI;

namespace Avalonia.Plot.ViewModel.Implementation;

internal sealed class PlotViewModel : ReactiveObject, IPlotViewModel
{
    private const double TOLERANCE = 0.0001;
    private readonly ObservableCollection<IPlotSeries> _series = new();
    private bool _autoScale = true;
    private Color _background = Colors.White;
    private Color _gridLinesColor = Colors.DimGray;
    private double _xMax = 10.0;
    private double _xMin;
    private double _xStep = 100;
    private double _yMax = 10.0;
    private double _yMin;
    private double _yStep = 0.2;

    public Color GridLinesColor
    {
        get => _gridLinesColor;
        set
        {
            if (_gridLinesColor == value)
                return;

            _gridLinesColor = value;

            this.RaisePropertyChanged();
        }
    }

    public Color Background
    {
        get => _background;
        set
        {
            if (_background == value)
                return;

            _background = value;

            this.RaisePropertyChanged();
        }
    }

    public IEnumerable<IPlotSeries> Series => _series;

    public void AddSeries(IPlotSeries series)
    {
        if (_series.Contains(series))
            return;

        series.Name ??= _series.Count == 0
            ? "Series"
            : $"Series {_series.Count}";

        _series.Add(series);
        series.PropertyChanged += SeriesOnPropertyChanged;
        AdjustScale();
    }

    public void RemoveSeries(IPlotSeries series)
    {
        _series.Remove(series);
        series.PropertyChanged -= SeriesOnPropertyChanged;
        AdjustScale();
    }

    public double XMin
    {
        get => _xMin;
        set
        {
            if (AutoScale)
                return;

            if (Math.Abs(_xMin - value) < TOLERANCE)
                return;

            _xMin = value;

            this.RaisePropertyChanged();
        }
    }

    public double XMax
    {
        get => _xMax;
        set
        {
            if (AutoScale)
                return;

            if (Math.Abs(_xMax - value) < TOLERANCE)
                return;

            _xMax = value;

            this.RaisePropertyChanged();
        }
    }

    public double XAxisStep
    {
        get => _xStep;
        set
        {
            if (Math.Abs(_xStep - value) < TOLERANCE)
                return;

            _xStep = value;

            this.RaisePropertyChanged();
        }
    }

    public double YMin
    {
        get => _yMin;
        set
        {
            if (AutoScale)
                return;

            if (Math.Abs(_yMin - value) < TOLERANCE)
                return;

            _yMin = value;

            this.RaisePropertyChanged();
        }
    }

    public double YMax
    {
        get => _yMax;
        set
        {
            if (AutoScale)
                return;

            if (Math.Abs(_yMax - value) < TOLERANCE)
                return;

            _yMax = value;

            this.RaisePropertyChanged();
        }
    }

    public double YAxisStep
    {
        get => _yStep;
        set
        {
            if (Math.Abs(_yStep - value) < TOLERANCE)
                return;

            _yStep = value;

            this.RaisePropertyChanged();
        }
    }

    public bool AutoScale
    {
        get => _autoScale;
        set
        {
            if (_autoScale == value)
                return;

            _autoScale = value;

            AdjustScale();
            this.RaisePropertyChanged();
        }
    }

    public event EventHandler? RedrawSuggested;

    private void SeriesOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IPlotSeries.XValues) or nameof(IPlotSeries.YValues))
        {
            AdjustScale();
            return;
        }

        if (e.PropertyName
            is nameof(IPlotSeries.Color)
            or nameof(IPlotSeries.LineStyle)
            or nameof(IPlotSeries.Thickness))
            RedrawSuggested?.Invoke(this, EventArgs.Empty);
    }

    private void AdjustScale()
    {
        if (!AutoScale)
            return;

        if (!Series.Any())
            return;

        var minX = double.PositiveInfinity;
        var maxX = double.NegativeInfinity;

        var minY = double.PositiveInfinity;
        var maxY = double.NegativeInfinity;

        foreach (var series in Series)
        {
            if (!series.XValues.Any() || !series.YValues.Any())
                continue;
            
            minX = Math.Min(minX, series.XValues.Min());
            maxX = Math.Max(maxX, series.XValues.Max());

            minY = Math.Min(minY, series.YValues.Min());
            maxY = Math.Max(maxY, series.YValues.Max());
        }

        _xMin = minX;
        _xMax = maxX;

        _yMin = minY;
        _yMax = maxY;

        RedrawSuggested?.Invoke(this, EventArgs.Empty);
    }
}