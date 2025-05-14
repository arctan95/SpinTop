using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DeskToys.Core.Services;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using SysBitmap = System.Drawing.Bitmap;

namespace DeskToys.Desktop.Windows.Services;

public class ScreenCapturer : IScreenCapturer
{
    public void CaptureScreen(int width, int height, Action<Bitmap?> callback)
    {
        using var bmp = new SysBitmap(width, height);
        using (var g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
        }

        callback(ToAvaloniaBitmap(bmp));
    }

    private Bitmap? ToAvaloniaBitmap(SysBitmap bitmap)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(memoryStream);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error converting to avalonia bitmap: {ex.Message}");
            return null;
        }
    }
}