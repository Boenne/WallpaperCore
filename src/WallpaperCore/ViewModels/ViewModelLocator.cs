namespace WallpaperCore.ViewModels;

public class ViewModelLocator
{
    public IMainViewModel MainViewModel => IoCContainer.Resolve<IMainViewModel>();
    public ISettingsViewModel SettingsViewModel => IoCContainer.Resolve<ISettingsViewModel>();
    public IBookmarksViewModel BookmarksViewModel => IoCContainer.Resolve<IBookmarksViewModel>();
    public IConfirmationViewModel ConfirmationViewModel => IoCContainer.Resolve<IConfirmationViewModel>();
}