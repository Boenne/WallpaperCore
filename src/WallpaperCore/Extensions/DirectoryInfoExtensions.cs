using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WallpaperCore.Extensions;

public static class DirectoryInfoExtensions
{
    public static List<FileInfo> GetImageFiles(this DirectoryInfo directoryInfo)
    {
        return directoryInfo.GetFiles().Where(x =>
            x.FullName.ToLower().EndsWith(".jpg") || x.FullName.ToLower().EndsWith(".jpeg") || x.FullName.ToLower().EndsWith(".png")).ToList();
    }
}