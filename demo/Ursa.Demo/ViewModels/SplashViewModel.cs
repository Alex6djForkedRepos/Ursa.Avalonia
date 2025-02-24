using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Irihi.Avalonia.Shared.Contracts;

namespace Ursa.Demo.ViewModels;

public partial class SplashViewModel: ObservableObject, IDialogContext
{
    [ObservableProperty] private double _progress;
    private Random _r = new();

    public SplashViewModel()
    {
        DispatcherTimer.Run(OnUpdate, TimeSpan.FromMilliseconds(200), DispatcherPriority.Default);
    }

    private bool OnUpdate()
    {
        Progress += 10 * _r.NextDouble();
        if (Progress <= 100)
        {
            return true;
        }
        else
        {
            RequestClose?.Invoke(this, null);
            return false;
        }
    }
    
    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
}