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
    private const double SCALE_STEP = 0.1D;
    private readonly SkiaDrawingOperation _drawOperation = new();

    private IPlotViewModel? _plot;

    private bool _pointerCaptured;
    private Point _startDragPoint;

    private Rect _startRect;

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
        if (!IsVisible)
            return;
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

        var bounds = canvas.LocalClipBounds;

        var xMin = _plot.XMin;
        var xMax = _plot.XMax;
        var width = xMax - xMin;

        var yMin = _plot.YMin;
        var yMax = _plot.YMax;
        var height = yMax - yMin;

        using var paint = new SKPaint();
        
        paint.Color = _plot.GridLinesColor.ToSKColor();
        paint.StrokeWidth = 1;

        var pixelXStep = 25;
        
        var stepX = pixelXStep * width / bounds.Width;

        var offsetX = ((int)(xMin / stepX) - 1) * stepX;

        var pixelOffsetX = bounds.Left + (offsetX - xMin) / width * bounds.Width;

        for (; pixelOffsetX <= bounds.Right; pixelOffsetX += pixelXStep)
            canvas.DrawLine((float)pixelOffsetX, bounds.Top, (float)pixelOffsetX, bounds.Bottom, paint);

        var pixelYStep = 25;
        
        var stepY = pixelYStep * height / bounds.Height;

        var offsetY = ((int)(yMin / stepY) - 1) * stepY;

        var pixelOffsetY = bounds.Bottom - (offsetY - yMin) / width * bounds.Height;

        for (; pixelOffsetY >= bounds.Top; pixelOffsetY -= pixelYStep)
            canvas.DrawLine(bounds.Left, (float)pixelOffsetY, bounds.Right, (float)pixelOffsetY, paint);

        paint.Style = SKPaintStyle.Stroke;
        paint.IsAntialias = true;

        foreach (var series in plotSeries)
        {
            if (series.XValues.Length != series.YValues.Length || !series.XValues.Any())
                continue;
            
            var points = EnumeratePoints(series, xMin, xMax, yMin, yMax, bounds);

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

    private IEnumerable<SKPoint> EnumeratePoints(IPlotSeries series, double xmin, double xmax, double ymin, double ymax,
        SKRect bounds)
    {
        var xs = series.XValues;
        var ys = series.YValues;

        var sx = (float)(bounds.Width / (xmax - xmin));
        var sy = (float)(bounds.Height / (ymax - ymin));

        var left = bounds.Left;
        var bot = bounds.Bottom;

        var start = xs.QuickSearch(x => x >= xmin) - 1;

        var end = xs.QuickSearch(x => x > xmax) + 2;

        if (start < 0)
            start = 0;

        if (end > xs.Length)
            end = xs.Length;

        var lowY = ys[start];
        var highY = ys[start];

        var prevX = (int)(left + (xs[start] - xmin) * sx);

        for (int i = start + 1; i < end; i++)
        {
            var screenX = (int)(left + (xs[i] - xmin) * sx);

            if (screenX == prevX)
            {
                lowY = Math.Min(ys[i], lowY);
                highY = Math.Max(ys[i], highY);

                continue;
            }

            lowY = bot - (lowY - ymin) * sy;
            yield return new SKPoint(prevX, (float)lowY);

            highY = bot - (highY - ymin) * sy;

            if (Math.Abs(lowY - highY) >= 1)
                yield return new SKPoint(prevX, (float)highY);

            prevX = screenX;
            lowY = ys[i];
            highY = ys[i];
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (_plot is null)
            return;

        var deltaY = e.Delta.Y;
        if (deltaY == 0)
            return;

        var positionBefore = e.GetPosition(this);

        var delta = 1 + deltaY * SCALE_STEP / Math.Abs(deltaY);

        switch (e.KeyModifiers)
        {
            case KeyModifiers.Shift:
                ScrollX(positionBefore, delta);
                break;

            case KeyModifiers.Control:
                ScrollY(positionBefore, delta);
                break;
            default:
                ScrollX(positionBefore, delta);
                ScrollY(positionBefore, delta);
                break;
        }

        base.OnPointerWheelChanged(e);
    }

    private void ScrollY(Point position, double delta)
    {
        var yMin = _plot!.YMin;
        var yMax = _plot.YMax;

        var sy = Bounds.Height / (yMax - yMin);

        var uy = yMin + (Bounds.Bottom - position.Y) / sy;
        sy *= delta;
        var dy = yMax - yMin;

        dy = (dy / delta - dy) / 2;

        yMin -= dy;
        yMax += dy;

        var y = Bounds.Bottom - (uy - yMin) * sy;

        var offsetY = (position.Y - y) / sy;

        yMin += offsetY;
        yMax += offsetY;

        _plot.YMin = yMin;
        _plot.YMax = yMax;
    }

    private void ScrollX(Point position, double delta)
    {
        var xMin = _plot!.XMin;
        var xMax = _plot.XMax;

        var sx = Bounds.Width / (xMax - xMin);

        var ux = xMin + (position.X - Bounds.Left) / sx;
        sx *= delta;

        var dx = xMax - xMin;
        dx = (dx / delta - dx) / 2;

        xMin -= dx;
        xMax += dx;

        var x = Bounds.Left + (ux - xMin) * sx;
        var offsetX = -(position.X - x) / sx;

        xMin += offsetX;
        xMax += offsetX;

        _plot.XMin = xMin;
        _plot.XMax = xMax;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
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

        base.OnPointerMoved(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (_plot is null)
            return;

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _pointerCaptured = true;

        _startRect = new Rect(_plot.XMin, _plot.YMin, _plot.XMax - _plot.XMin, _plot.YMax - _plot.YMin);
        _startDragPoint = e.GetPosition(this);

        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _pointerCaptured = false;
    }
}