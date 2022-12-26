namespace WallpaperCore.Services;

public interface IWallpaperManagerWrapper
{
    void Set(string uri);
}

public class WallpaperManagerWrapper : IWallpaperManagerWrapper
{
    public void Set(string uri)
    {
        WallpaperManager.Set(uri, WallpaperManager.Style.Centered);
    }
}