using System;
using Avalonia.Media.Imaging;

namespace DeskToys.Core.Services;

public interface IScreenCapturer
{
    public void CaptureScreen(int width, int height, Action<Bitmap?> callback);
}