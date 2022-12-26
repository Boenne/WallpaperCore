using WallpaperCore.ViewModels;

namespace WallpaperCore;

public class ViewModelLocator
{
    public IMainViewModel MainViewModel => IoCContainer.Resolve<IMainViewModel>();
    public ISettingsViewModel SettingsViewModel => IoCContainer.Resolve<ISettingsViewModel>();
}