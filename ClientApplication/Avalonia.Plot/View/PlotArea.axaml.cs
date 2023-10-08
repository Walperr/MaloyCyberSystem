using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Plot.Misc;
using Avalonia.Plot.ViewModel;
using Avalonia.Skia;
using SkiaSharp;

namespace Avalonia.Plot.View;

public partial class PlotArea : UserControl
{
    private readonly SkiaDrawingOperation _drawOperation = new();

    private IPlotViewModel? _plot;

    public PlotArea()
    {
        InitializeComponent();

        _drawOperation.OnRender += OnRender;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_plot is not null)
        {
            _plot.RedrawSuggested -= PlotOnRedrawSuggested;
            _plot.PropertyChanged -= PlotOnPropertyChanged;
        }

        _plot = DataContext as IPlotViewModel;

        if (_plot is not null)
        {
            _plot.RedrawSuggested += PlotOnRedrawSuggested;
            _plot.PropertyChanged += PlotOnPropertyChanged;
        }

        InvalidateVisual();
    }

    private void PlotOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void PlotOnRedrawSuggested(object? sender, EventArgs e)
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        _drawOperation.Bounds = Bounds;
        context.Custom(_drawOperation);
    }

    private void OnRender(SKCanvas canvas)
    {
        if (_plot is null)
            return;

        canvas.Clear(_plot.Background.ToSKColor());

        var plotSeries = _plot.Series;

        var xMin = _plot.XMin;
        var xMax = _plot.XMax;
        var width = xMax - xMin;
        
        var yMin = _plot.YMin;
        var yMax = _plot.YMax;
        var height = yMax - yMin;

        using var paint = new SKPaint();

        var xStep = _plot.XStep;
        var yStep = _plot.YStep;

        var xCount = (int)(width / xStep);
        var yCount = (int)(height / yStep);
        
        paint.Color = _plot.GridLinesColor.ToSKColor();
        paint.StrokeWidth = 1;
        
        for (int i = -xCount; i < xCount; i++)
        {
            var x = xStep * i;
            
            var scaledX = (float) (Bounds.Left + (x - xMin) * Bounds.Width / width);
            canvas.DrawLine(scaledX, (float) Bounds.Top, scaledX, (float) Bounds.Bottom, paint);
        }
        
        for (int i = -yCount; i < yCount; i++)
        {
            var y = yStep * i;
            
            var scaledY = (float) (Bounds.Bottom - (y - yMin) * Bounds.Height / height);
            canvas.DrawLine((float) Bounds.Left, scaledY, (float) Bounds.Right, scaledY, paint);
        }

        paint.Style = SKPaintStyle.Stroke;
        
        foreach (var series in plotSeries)
        {
            var points = ToScreenPoints(series.XValues, series.YValues, xMin, width, yMin, height);

            paint.Color = series.Color.ToSKColor();
            paint.PathEffect = SKPathEffect.CreateDash(series.LineStyle.ToDashArray(), 0);
            paint.StrokeWidth = series.Thickness;

            var path = new SKPath();

            var move = true;

            foreach (var point in points)
            {
                if (move)
                {
                    path.MoveTo(point);
                    move = false;
                    continue;
                }

                path.LineTo(point);
            }
            
            canvas.DrawPath(path, paint);
        }
    }

    private SKPoint[] ToScreenPoints(IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double minX,
        double width,
        double minY,
        double height)
    {
        var points = new SKPoint[x.Count];
        
        var sx = Bounds.Width / width;
        var sy = Bounds.Height / height;

        Parallel.For(0, points.Length, i =>
        {
            var scaledX = (float) (Bounds.Left + (x[i] - minX) * sx);
            var scaledY = (float) (Bounds.Bottom - (y[i] - minY) * sy);

            points[i] = new SKPoint(scaledX, scaledY);
        });

        return points;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_plot is null)
            return;
        
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        
        if (!_pointerCaptured)
            return;
        
        var position = e.GetPosition(this);
        
        var sx = (_plot.XMax - _plot.XMin) / Bounds.Width;
        var sy = (_plot.YMax - _plot.YMin) / Bounds.Height;
        
        var dx = -(position.X - _startDragPoint.X) * sx;
        var dy = (position.Y - _startDragPoint.Y) * sy;

        _plot.XMin = _startRect.Left + dx;
        _plot.XMax = _startRect.Right + dx;
        
        _plot.YMin = _startRect.Top + dy;
        _plot.YMax = _startRect.Bottom + dy;
    }

    private Rect _startRect;
    private Point _startDragPoint;

    private bool _pointerCaptured;

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_plot is null)
            return;
        
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        
        _pointerCaptured = true;

        _startRect = new Rect(_plot.XMin, _plot.YMin, _plot.XMax - _plot.XMin, _plot.YMax - _plot.YMin);
        _startDragPoint = e.GetPosition(this);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _pointerCaptured = false;
    }
}