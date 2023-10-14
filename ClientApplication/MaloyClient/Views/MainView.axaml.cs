using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MaloyClient.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void LastNotification_OnDataContextChanged(object? sender, EventArgs e)
    {
        Border.Opacity = 0;
        Border.Opacity = 1;
        Border.Opacity = 0;
    }
}