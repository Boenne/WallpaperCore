using System.Windows;
using WallpaperCore.Services;
using WallpaperCore.Services.Messenger;
using WallpaperCore.ViewModels;

namespace WallpaperCore;

public partial class App : Application
{
    public App()
    {
        IoCContainer.Register<IMessengerService, MessengerService>();
        IoCContainer.Register<IDispatcherService, DispatcherService>();
        IoCContainer.Register<IConfirmationService, ConfirmationService>();
        IoCContainer.Register<IWallpaperService, WallpaperService>();
        IoCContainer.Register<IImageService, ImageService>();
        IoCContainer.Register<IBackgroundWorker, BackgroundWorker>();

        IoCContainer.Register<IMainViewModel, MainViewModel>();
        IoCContainer.Register<ISettingsViewModel, SettingsViewModel>();
        IoCContainer.Register<IBookmarksViewModel, BookmarksViewModel>();
        IoCContainer.Register<IConfirmationViewModel, ConfirmationViewModel>();
    }
}