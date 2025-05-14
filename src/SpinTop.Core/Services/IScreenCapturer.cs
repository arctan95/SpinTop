using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace SpinTop.Core.Services;

public interface IScreenCapturer
{
    public Task<Bitmap?> CaptureScreen(int width, int height);
}