namespace Avalonia.Plot.Misc;

public static class Extensions
{
    public static float[] ToDashArray(this LineStyle lineStyle)
    {
        return lineStyle switch
        {
            LineStyle.Solid => null,
            LineStyle.Dash => new float[] { 3, 2 },
            LineStyle.DashDot => new float[] { 3, 1, 1, 1 },
            LineStyle.DashDotDot => new float[] { 3, 1, 1, 1, 1, 1 },
            LineStyle.Dot => new float[] { 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(lineStyle), lineStyle, null)
        } ?? Array.Empty<float>();
    }
}