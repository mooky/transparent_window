namespace transparent_window;

using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public partial class MainForm : Form
{
    private const double ImageScale = 0.45;

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
    private const int HTCAPTION          =  2;

    private Bitmap? _monkey;
    private bool[,]? _opaque;

    public MainForm()
    {
        InitializeComponent();

        FormBorderStyle = FormBorderStyle.None;
        BackColor       = Color.White;
        TransparencyKey = Color.White;
        ShowInTaskbar   = false;
        TopMost         = true;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        var imagePath = Path.Combine(AppContext.BaseDirectory, "context", "images", "monkey.png");
        if (!File.Exists(imagePath))
        {
            MessageBox.Show($"Image not found:\n{imagePath}", "transparent_window",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
            return;
        }

        double scale = ImageScale * (DeviceDpi / 96.0);
        _monkey    = LoadScaled(imagePath, scale);
        _opaque    = BuildOpacityMap(_monkey);
        ClientSize = _monkey.Size;
    }

    private static Bitmap LoadScaled(string path, double scale)
    {
        using var original = new Bitmap(path);
        var size   = new Size((int)(original.Width * scale), (int)(original.Height * scale));
        var scaled = new Bitmap(size.Width, size.Height);
        using var g = Graphics.FromImage(scaled);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.DrawImage(original, 0, 0, size.Width, size.Height);
        return scaled;
    }

    private static bool[,] BuildOpacityMap(Bitmap bmp)
    {
        var map  = new bool[bmp.Width, bmp.Height];
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            int stride = Math.Abs(data.Stride);
            var pixels = new byte[stride * bmp.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
            for (int y = 0; y < bmp.Height; y++)
            for (int x = 0; x < bmp.Width;  x++)
            {
                int i = y * stride + x * 4;
                byte b = pixels[i], g = pixels[i + 1], r = pixels[i + 2];
                map[x, y] = !(r >= 250 && g >= 250 && b >= 250);
            }
        }
        finally
        {
            bmp.UnlockBits(data);
        }
        return map;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_NOACTIVATE;
            return cp;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_monkey != null)
            e.Graphics.DrawImage(_monkey, 0, 0);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_MOUSEACTIVATE)   { HandleMouseActivate(ref m); return; }
        if (m.Msg == WM_SYSCOMMAND && IsSuppressedCommand(m)) return;
        if (m.Msg == WM_NCLBUTTONDBLCLK) { Application.Exit(); return; }
        if (m.Msg == WM_NCHITTEST)       { HandleNcHitTest(ref m); return; }

        base.WndProc(ref m);
    }

    private static void HandleMouseActivate(ref Message m) =>
        m.Result = (IntPtr)MA_NOACTIVATE;

    private static bool IsSuppressedCommand(Message m)
    {
        int cmd = m.WParam.ToInt32() & 0xFFF0;
        return cmd == SC_MINIMIZE || cmd == SC_MAXIMIZE || cmd == SC_RESTORE;
    }

    private void HandleNcHitTest(ref Message m)
    {
        base.WndProc(ref m);
        if (_opaque == null) return;

        int lp     = m.LParam.ToInt32();
        var client = PointToClient(new Point((short)(lp & 0xFFFF), (short)(lp >> 16)));

        bool isOpaque = client.X >= 0 && client.X < _monkey!.Width &&
                        client.Y >= 0 && client.Y < _monkey!.Height &&
                        _opaque[client.X, client.Y];

        m.Result = (IntPtr)(isOpaque ? HTCAPTION : HTTRANSPARENT);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _monkey?.Dispose();
        base.Dispose(disposing);
    }
}
