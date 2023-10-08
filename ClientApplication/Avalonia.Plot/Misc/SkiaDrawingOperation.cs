using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace Avalonia.Plot.Misc;

internal sealed class SkiaDrawingOperation : ICustomDrawOperation
{
    public Action<SKCanvas>? OnRender;

    public bool Equals(ICustomDrawOperation? other)
    {
        return other == this;
    }

    public void Dispose()
    {
        // do nothing
    }

    //todo: implement hits for different series
    public bool HitTest(Point p)
    {
        return true;
    }

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null)
            return;

        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;

        Render(canvas);
    }

    public Rect Bounds { get; set; }

    private void Render(SKCanvas canvas)
    {
        OnRender?.Invoke(canvas);
    }
}