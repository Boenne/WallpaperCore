using System.Windows;
using WallpaperCore.Services;
using WallpaperCore.ViewModels;

namespace WallpaperCore;

public partial class App : Application
{
    public App()
    {
        IoCContainer.Register<IDispatcherWrapper, DispatcherWrapper>();
        IoCContainer.Register<IWallpaperManagerWrapper, WallpaperManagerWrapper>();
        IoCContainer.Register<IImageService, ImageService>();

        IoCContainer.Register<IMainViewModel, MainViewModel>();
        IoCContainer.Register<ISettingsViewModel, SettingsViewModel>();
    }
}