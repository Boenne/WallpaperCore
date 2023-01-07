using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace WallpaperCore.Services;

public interface IImageService
{
    BitmapImage CreatePreviewImage(string path);
    string GetImagePath(string filePath, string tempFolder);
}

public class ImageService : IImageService
{
    private FileStream? _bitmapStreamSource;

    public BitmapImage CreatePreviewImage(string path)
    {
        var bitmap = new BitmapImage();
        _bitmapStreamSource = File.OpenRead(path);
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
        bitmap.StreamSource = _bitmapStreamSource;
        bitmap.EndInit();
        bitmap.Freeze();

        ClearBitmapStream();

        return bitmap;
    }

    public string GetImagePath(string filePath, string tempFolder)
    {
        return IsLandscape(filePath)
            ? filePath
            : ConvertPortraitImageToLandscape(filePath, tempFolder);
    }

    public void ClearBitmapStream()
    {
        if (_bitmapStreamSource == null) return;
        _bitmapStreamSource.Close();
        _bitmapStreamSource.Dispose();
        _bitmapStreamSource = null;
        GC.Collect();
    }

    public string ConvertPortraitImageToLandscape(string filePath, string tempFolder)
    {
        const int width = 2560;
        const int height = 1440;
        var image = Image.FromFile(filePath);
        var rightImageRatio = image.Width / (double)image.Height;

        var fileInfo = new FileInfo(filePath);
        var tempFile = $"{tempFolder}/{fileInfo.Name}";
        if (File.Exists(tempFile)) return tempFile;

        using (image)
        {
            using (var bitmap = new Bitmap(width, height))
            {
                using (var canvas = Graphics.FromImage(bitmap))
                {
                    canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    var rightSideWith = (int)(height * rightImageRatio);
                    var leftSideWith = width - rightSideWith;
                    var ratio = leftSideWith / (double)height;

                    canvas.DrawImage(image,
                        new Rectangle(0,
                            0,
                            leftSideWith,
                            height),
                        new Rectangle(0,
                            0,
                            image.Width,
                            (int)(image.Width / ratio)),
                        GraphicsUnit.Pixel);
                    canvas.DrawImage(image,
                        new Rectangle(width - rightSideWith,
                            0,
                            rightSideWith,
                            height),
                        new Rectangle(0,
                            0,
                            image.Width,
                            image.Height),
                        GraphicsUnit.Pixel);
                    canvas.Save();
                }

                bitmap.Save(tempFile, ImageFormat.Jpeg);
                return tempFile;
            }
        }
    }

    public bool IsLandscape(string filePath)
    {
        using var bitmap = new Bitmap(filePath);
        return bitmap.Width >= bitmap.Height;
    }
}