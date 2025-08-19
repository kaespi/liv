using liv.Services;

namespace liv;

public partial class MainWindow : Form
{
    private readonly IImageBuffer _imageBuffer;
    private bool _isFullScreen = false;
    private FormWindowState _previousWindowState;
    private FormBorderStyle _previousBorderStyle;
    private Rectangle _previousBounds;
    private PictureBox? pictureBox = null;

    public MainWindow()
    {
        InitializeCustomComponents();
        
        _imageBuffer = new ImageBuffer();
        _imageBuffer.BufferChanged += OnBufferChanged;
        
        this.BackColor = Color.Black;
        this.WindowState = FormWindowState.Maximized;
        this.KeyPreview = true;
        
        this.KeyDown += OnKeyDown;
        this.Resize += OnResize;

        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && File.Exists(args[1]))
        {
            _imageBuffer.Initialize(args[1]);
        }
    }

    private void InitializeCustomComponents()
    {
        pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black
        };
        Controls.Add(pictureBox);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.F11:
                ToggleFullScreen();
                break;
            case Keys.Right:
                _ = _imageBuffer.MoveNextAsync();
                break;
            case Keys.Left:
                _ = _imageBuffer.MovePreviousAsync();
                break;
            case Keys.Escape:
                if (_isFullScreen)
                    ToggleFullScreen();
                break;
        }
    }

    private void OnBufferChanged(object? sender, ImageBufferChangedEventArgs e)
    {
        if (pictureBox == null) return;
        if (pictureBox.InvokeRequired)
        {
            pictureBox.Invoke(() => UpdateImage(e.Image));
        }
        else
        {
            UpdateImage(e.Image);
        }
    }

    private void UpdateImage(Image? image)
    {
        if (pictureBox == null) return;
        if (pictureBox.Image != null)
        {
            var oldImage = pictureBox.Image;
            pictureBox.Image = null;
            oldImage.Dispose();
        }
        pictureBox.Image = image;
    }

    private void ToggleFullScreen()
    {
        if (!_isFullScreen)
        {
            _previousWindowState = WindowState;
            _previousBorderStyle = FormBorderStyle;
            _previousBounds = Bounds;

            if (Screen.PrimaryScreen != null)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                Bounds = Screen.PrimaryScreen.Bounds;
            }
        }
        else
        {
            FormBorderStyle = _previousBorderStyle;
            WindowState = _previousWindowState;
            Bounds = _previousBounds;
        }

        _isFullScreen = !_isFullScreen;
    }

    private void OnResize(object? sender, EventArgs e)
    {
        if (pictureBox != null && pictureBox.Image != null)
        {
            pictureBox.Invalidate();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (_imageBuffer is IDisposable disposableBuffer)
            disposableBuffer.Dispose();
    }
}
