using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace WallpaperCore.Services;

public interface IDispatcherService
{
    Task DoUIWork(Action action);
}

public class DispatcherService : IDispatcherService
{
    public async Task DoUIWork(Action action)
    {
        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
    }
}