using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace WallpaperCore.Wrappers;

public interface IDispatcherWrapper
{
    Task BeginInvoke(Action action);
}

public class DispatcherWrapper : IDispatcherWrapper
{
    public async Task BeginInvoke(Action action)
    {
        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
    }
}