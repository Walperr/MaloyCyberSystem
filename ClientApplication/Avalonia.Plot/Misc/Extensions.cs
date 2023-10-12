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
    
    public static int QuickSearch<T>(this IReadOnlyList<T>? items, Predicate<T> search,
        int? start = null, int? end = null)
    {
        if (items is null || items.Count == 0)
            return -1;

        var min = start ?? 0;
        var startIndex = min;
        var max = end ?? items.Count - 1;
        var endIndex = max;

        while (startIndex <= endIndex && !search(items[startIndex]))
        {
            var middle = (startIndex + endIndex) / 2;
            if (search(items[middle]))
                endIndex = middle;
            else
                startIndex = middle + 1;
        }

        if (endIndex <= min)
            return -1;

        if (startIndex >= max + 1)
            return max + 1;

        return startIndex;
    }
}