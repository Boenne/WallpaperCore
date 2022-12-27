namespace WallpaperCore.Models;

public class Bookmark
{
    public Bookmark()
    {
    }

    public Bookmark(string title, string path)
    {
        Title = title;
        Path = path;
    }

    public string Title { get; set; }
    public string Path { get; set; }
}