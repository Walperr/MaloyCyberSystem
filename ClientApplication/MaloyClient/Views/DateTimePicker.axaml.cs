using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace MaloyClient.Views;

public sealed partial class DateTimePicker : UserControl
{
    public static readonly StyledProperty<DateTime> CurrentDateTimeProperty =
        AvaloniaProperty.Register<DateTimePicker, DateTime>(
            nameof(CurrentDateTime), defaultBindingMode: BindingMode.TwoWay, defaultValue: DateTime.Today);

    public static readonly StyledProperty<DateTime> CurrentDateProperty =
        AvaloniaProperty.Register<DateTimePicker, DateTime>(
            "CurrentDate", DateTime.Today);

    public static readonly StyledProperty<string> TimeProperty = AvaloniaProperty.Register<DateTimePicker, string>(
        "Time", DateTime.Now.TimeOfDay.ToString());

    private int _syncLocks;

    static DateTimePicker()
    {
        CurrentDateTimeProperty.Changed.AddClassHandler<DateTimePicker, DateTime>((s, e) =>
        {
            if (Interlocked.CompareExchange(ref s._syncLocks, 1, 0) == 1)
                return;
            try
            {
                s.CurrentDate = e.NewValue.Value.Date;
                s.Time = e.NewValue.Value.TimeOfDay.ToString();
            }
            finally
            {
                Interlocked.Exchange(ref s._syncLocks, 0);
            }
        });

        CurrentDateProperty.Changed.AddClassHandler<DateTimePicker, DateTime>((s, e) =>
        {
            if (Interlocked.CompareExchange(ref s._syncLocks, 1, 0) == 1)
                return;
            try
            {
                if (!TimeSpan.TryParse(s.Time, out var timeSpan))
                    timeSpan = TimeSpan.Zero;

                s.CurrentDateTime = e.NewValue.Value.Date + timeSpan;
            }
            finally
            {
                Interlocked.Exchange(ref s._syncLocks, 0);
            }
        });

        TimeProperty.Changed.AddClassHandler<DateTimePicker, string>((s, e) =>
        {
            if (Interlocked.CompareExchange(ref s._syncLocks, 1, 0) == 1)
                return;
            try
            {
                if (!TimeSpan.TryParse(e.NewValue.Value, out var timeSpan))
                    timeSpan = TimeSpan.Zero;

                s.CurrentDateTime = s.CurrentDate.Date + timeSpan;
                s.Time = timeSpan.ToString();
            }
            finally
            {
                Interlocked.Exchange(ref s._syncLocks, 0);
            }
        });
    }

    public DateTimePicker()
    {
        InitializeComponent();
    }

    public string Time
    {
        get => GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public DateTime CurrentDate
    {
        get => GetValue(CurrentDateProperty);
        set => SetValue(CurrentDateProperty, value);
    }

    public DateTime CurrentDateTime
    {
        get => GetValue(CurrentDateTimeProperty);
        set => SetValue(CurrentDateTimeProperty, value);
    }

    private void TextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) 
            Time = TextBox.Text ?? string.Empty;
    }

    private void TextBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        Time = TextBox.Text ?? string.Empty;
    }
}