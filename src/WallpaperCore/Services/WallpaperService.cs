namespace WallpaperCore.Services;

public interface IWallpaperService
{
    void Set(string uri);
}

public class WallpaperService : IWallpaperService
{
    public void Set(string uri)
    {
        WallpaperManager.Set(uri, WallpaperManager.Style.Centered);
    }
}