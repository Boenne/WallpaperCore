using System.Windows;
using WallpaperCore.Services;
using WallpaperCore.ViewModels;
using WallpaperCore.Wrappers;
using WallpaperCore.Wrappers.Messenger;

namespace WallpaperCore;

public partial class App : Application
{
    public App()
    {
        IoCContainer.Register<IMessengerWrapper, MessengerWrapper>();
        IoCContainer.Register<IDispatcherWrapper, DispatcherWrapper>();
        IoCContainer.Register<IMessageBoxWrapper, MessageBoxWrapper>();
        IoCContainer.Register<IWallpaperService, WallpaperService>();
        IoCContainer.Register<IImageService, ImageService>();

        IoCContainer.Register<IMainViewModel, MainViewModel>();
        IoCContainer.Register<ISettingsViewModel, SettingsViewModel>();
        IoCContainer.Register<IBookmarksViewModel, BookmarksViewModel>();
    }
}