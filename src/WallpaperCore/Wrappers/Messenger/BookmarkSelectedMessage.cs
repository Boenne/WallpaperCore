namespace WallpaperCore.Wrappers.Messenger;

public class BookmarkSelectedMessage
{
    public string Path { get; set; }

    public BookmarkSelectedMessage(string path)
    {
        Path = path;
    }
}