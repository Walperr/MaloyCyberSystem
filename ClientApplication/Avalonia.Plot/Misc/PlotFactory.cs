using Avalonia.Plot.ViewModel;
using Avalonia.Plot.ViewModel.Implementation;

namespace Avalonia.Plot.Misc;

public static class PlotFactory
{
    public static IPlotViewModel CreatePlot()
    {
        return new PlotViewModel();
    }

    public static IPlotSeries CreateSeries()
    {
        return new PlotSeries();
    }
}