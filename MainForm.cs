namespace transparent_window;

public partial class MainForm : Form
{
    private Bitmap? _monkey;

    public MainForm()
    {
        InitializeComponent();

        FormBorderStyle = FormBorderStyle.None;
        BackColor        = Color.White;
        TransparencyKey  = Color.White;
        ShowInTaskbar    = false;
        TopMost          = true;

        var imagePath = Path.Combine(AppContext.BaseDirectory, "context", "images", "monkey.png");
        using var original = new Bitmap(imagePath);
        var scaledSize = new Size((int)(original.Width * 0.45), (int)(original.Height * 0.45));
        _monkey = new Bitmap(scaledSize.Width, scaledSize.Height);
        using (var g = Graphics.FromImage(_monkey))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(original, 0, 0, scaledSize.Width, scaledSize.Height);
        }
        ClientSize = scaledSize;

    }

    protected override CreateParams CreateParams
    {
        get
        {
            const int WS_EX_NOACTIVATE = 0x08000000;
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
        const int WM_MOUSEACTIVATE = 0x0021;
        const int MA_NOACTIVATE    = 3;

        if (m.Msg == WM_MOUSEACTIVATE)
        {
            m.Result = (IntPtr)MA_NOACTIVATE;
            return;
        }

        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MINIMIZE   = 0xF020;
        const int SC_MAXIMIZE   = 0xF030;
        const int SC_RESTORE    = 0xF120;

        if (m.Msg == WM_SYSCOMMAND)
        {
            int cmd = m.WParam.ToInt32() & 0xFFF0;
            if (cmd == SC_MINIMIZE || cmd == SC_MAXIMIZE || cmd == SC_RESTORE)
                return; // swallow — never minimize, maximize, or restore
        }

        const int WM_NCLBUTTONDBLCLK = 0x00A3;

        if (m.Msg == WM_NCLBUTTONDBLCLK)
        {
            Application.Exit();
            return;
        }

        const int WM_NCHITTEST  = 0x0084;
        const int HTTRANSPARENT = -1;
        const int HTCAPTION     =  2;

        if (m.Msg == WM_NCHITTEST)
        {
            base.WndProc(ref m);

            if (_monkey != null)
            {
                int lp     = m.LParam.ToInt32();
                var client = PointToClient(new Point((short)(lp & 0xFFFF), (short)(lp >> 16)));

                if (client.X >= 0 && client.X < _monkey.Width &&
                    client.Y >= 0 && client.Y < _monkey.Height)
                {
                    var px = _monkey.GetPixel(client.X, client.Y);
                    if (px.R >= 250 && px.G >= 250 && px.B >= 250)
                        m.Result = (IntPtr)HTTRANSPARENT;  // click-through on transparent areas
                    else
                        m.Result = (IntPtr)HTCAPTION;      // drag-to-move on monkey pixels
                }
                else
                {
                    m.Result = (IntPtr)HTTRANSPARENT;
                }
            }
            return;
        }

        base.WndProc(ref m);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _monkey?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }
}
