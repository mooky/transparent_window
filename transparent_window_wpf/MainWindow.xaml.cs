namespace transparent_window_wpf;

using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public partial class MainWindow : Window
{
    private const double ImageScale = 0.45;

    private const int GWL_EXSTYLE        = -20;
    private const int WS_EX_NOACTIVATE   = 0x08000000;
    private const int WM_MOUSEACTIVATE   = 0x0021;
    private const int MA_NOACTIVATE      = 3;
    private const int WM_SYSCOMMAND      = 0x0112;
    private const int SC_MINIMIZE        = 0xF020;
    private const int SC_MAXIMIZE        = 0xF030;
    private const int SC_RESTORE         = 0xF120;
    private const int WM_NCLBUTTONDBLCLK = 0x00A3;
    private const int WM_NCHITTEST       = 0x0084;
    private const int HTTRANSPARENT      = -1;
    private const int HTCAPTION          = 2;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hWnd, ref POINT pt);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X, Y; }

    private bool[,]? _opaque;
    private int _imageWidth, _imageHeight;
    private IntPtr _hwnd;

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += OnSourceInitialized;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;

        int exStyle = GetWindowLong(_hwnd, GWL_EXSTYLE);
        SetWindowLong(_hwnd, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE);

        var src = HwndSource.FromHwnd(_hwnd);
        src.AddHook(WndProc);

        var imagePath = Path.Combine(AppContext.BaseDirectory, "context", "images", "monkey.png");
        if (!File.Exists(imagePath))
        {
            MessageBox.Show($"Image not found:\n{imagePath}", "transparent_window_wpf",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
            return;
        }

        var matrix = src.CompositionTarget.TransformToDevice;
        double dpiX = matrix.M11 * 96.0;
        double dpiY = matrix.M22 * 96.0;
        double scale = ImageScale * (dpiX / 96.0);

        var (bitmap, opaque) = LoadImage(imagePath, scale, dpiX, dpiY);
        _opaque      = opaque;
        _imageWidth  = bitmap.PixelWidth;
        _imageHeight = bitmap.PixelHeight;

        MonkeyImage.Source = bitmap;

        // Size window in DIPs so the bitmap renders 1:1 in physical pixels.
        Width  = _imageWidth  * 96.0 / dpiX;
        Height = _imageHeight * 96.0 / dpiY;
    }

    private static (BitmapSource bitmap, bool[,] opaque) LoadImage(
        string path, double scale, double dpiX, double dpiY)
    {
        var bi = new BitmapImage();
        bi.BeginInit();
        bi.UriSource    = new Uri(path, UriKind.Absolute);
        bi.CacheOption  = BitmapCacheOption.OnLoad;
        bi.EndInit();
        bi.Freeze();

        int dstW = (int)(bi.Width  * scale);
        int dstH = (int)(bi.Height * scale);

        // Render scaled image to a physical-pixel RenderTargetBitmap.
        var visual = new DrawingVisual();
        RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.HighQuality);
        using (var dc = visual.RenderOpen())
            dc.DrawImage(bi, new Rect(0, 0, dstW, dstH));

        var rtb = new RenderTargetBitmap(dstW, dstH, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(visual);

        // Convert pre-multiplied alpha to straight alpha for per-pixel access.
        var cvt = new FormatConvertedBitmap(rtb, PixelFormats.Bgra32, null, 0);
        int stride = dstW * 4;
        var pixels = new byte[stride * dstH];
        cvt.CopyPixels(pixels, stride, 0);

        // Build opacity map; un-premultiply kept pixels (FormatConvertedBitmap Pbgra32→Bgra32
        // copies bytes as-is without dividing RGB by alpha, so we correct that here).
        var opaque = new bool[dstW, dstH];
        for (int y = 0; y < dstH; y++)
        for (int x = 0; x < dstW; x++)
        {
            int  i  = y * stride + x * 4;
            byte a  = pixels[i + 3];
            bool op = a > 32;
            opaque[x, y] = op;
            if (!op)
            {
                pixels[i] = pixels[i + 1] = pixels[i + 2] = pixels[i + 3] = 0;
            }
            else if (a < 255)
            {
                pixels[i]     = (byte)Math.Min(255, (pixels[i]     * 255 + a / 2) / a);
                pixels[i + 1] = (byte)Math.Min(255, (pixels[i + 1] * 255 + a / 2) / a);
                pixels[i + 2] = (byte)Math.Min(255, (pixels[i + 2] * 255 + a / 2) / a);
            }
        }

        // Final bitmap carries screen DPI so WPF renders it pixel-perfect.
        var result = new WriteableBitmap(dstW, dstH, dpiX, dpiY, PixelFormats.Bgra32, null);
        result.WritePixels(new Int32Rect(0, 0, dstW, dstH), pixels, stride, 0);
        result.Freeze();

        return (result, opaque);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_MOUSEACTIVATE:
                handled = true;
                return (IntPtr)MA_NOACTIVATE;

            case WM_SYSCOMMAND:
                int cmd = wParam.ToInt32() & 0xFFF0;
                if (cmd == SC_MINIMIZE || cmd == SC_MAXIMIZE || cmd == SC_RESTORE)
                {
                    handled = true;
                    return IntPtr.Zero;
                }
                break;

            case WM_NCLBUTTONDBLCLK:
                Application.Current.Shutdown();
                handled = true;
                return IntPtr.Zero;

            case WM_NCHITTEST:
                return HandleNcHitTest(lParam, ref handled);
        }
        return IntPtr.Zero;
    }

    private IntPtr HandleNcHitTest(IntPtr lParam, ref bool handled)
    {
        if (_opaque == null) return IntPtr.Zero;

        int lp = (int)lParam;
        var pt = new POINT { X = (short)(lp & 0xFFFF), Y = (short)(lp >> 16) };
        ScreenToClient(_hwnd, ref pt);

        bool isOpaque = pt.X >= 0 && pt.X < _imageWidth &&
                        pt.Y >= 0 && pt.Y < _imageHeight &&
                        _opaque[pt.X, pt.Y];

        handled = true;
        return (IntPtr)(isOpaque ? HTCAPTION : HTTRANSPARENT);
    }
}
