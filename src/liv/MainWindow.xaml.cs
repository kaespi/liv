using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using liv.Core;

namespace liv;

/// <summary>
/// Main application window — displays a single image at a time and supports
/// navigation, zoom, pan, fullscreen toggle, and file deletion.
/// </summary>
public partial class MainWindow : Window
{
    // --- Core components --------------------------------------------------
    private readonly ImageNavigator _navigator;
    private readonly ImageCache<BitmapSource> _cache;
    private readonly ZoomController _zoom = new();
    private readonly string _folderPath;

    // --- FileSystemWatcher ------------------------------------------------
    private FileSystemWatcher? _watcher;

    // --- Fullscreen state -------------------------------------------------
    private bool _isFullScreen;
    private WindowState _savedWindowState;
    private WindowStyle _savedWindowStyle;
    private ResizeMode _savedResizeMode;
    private bool _savedTopmost;

    // --- Pan / drag state -------------------------------------------------
    private bool _isDragging;
    private Point _lastDragPoint;

    // --- Fitted image dimensions (at zoom = 1.0) --------------------------
    private double _fittedWidth;
    private double _fittedHeight;

    /// <summary>
    /// Creates the main window and initialises all components for the given image file.
    /// </summary>
    /// <param name="initialFilePath">Absolute path to the image to display first.</param>
    public MainWindow(string initialFilePath)
    {
        InitializeComponent();

        var fullPath = Path.GetFullPath(initialFilePath);
        _folderPath = Path.GetDirectoryName(fullPath)!;

        var files = ImageFileScanner.ScanFolder(_folderPath);
        int startIndex = FindFileIndex(files, fullPath);

        _navigator = new ImageNavigator(files, startIndex);
        _cache = new ImageCache<BitmapSource>(3, LoadBitmapAsync);

        SetupFileWatcher();
        Loaded += OnWindowLoaded;
    }

    // =====================================================================
    //  Image loading
    // =====================================================================

    private static int FindFileIndex(IReadOnlyList<string> files, string target)
    {
        for (int i = 0; i < files.Count; i++)
        {
            if (string.Equals(files[i], target, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return 0;
    }

    /// <summary>
    /// Loads and freezes a <see cref="BitmapSource"/> on a background thread.
    /// Applies EXIF orientation if present so the image is displayed correctly.
    /// Frozen bitmaps can be safely used from the UI thread.
    /// </summary>
    private static Task<BitmapSource?> LoadBitmapAsync(string filePath, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            if (!File.Exists(filePath)) return null;

            try
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(filePath, UriKind.Absolute);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                bi.Freeze();

                BitmapSource result = bi;

                // Read EXIF orientation from a BitmapFrame (tag 0x0112 = 274)
                var frame = BitmapFrame.Create(
                    new Uri(filePath, UriKind.Absolute),
                    BitmapCreateOptions.IgnoreColorProfile,
                    BitmapCacheOption.OnLoad);

                if (frame.Metadata is BitmapMetadata meta)
                {
                    var orientationObj = meta.GetQuery("/app1/ifd/{ushort=274}");
                    if (orientationObj is ushort orientation)
                        result = ApplyExifOrientation(bi, orientation);
                }

                return result;
            }
            catch
            {
                return null;
            }
        }, ct);
    }

    /// <summary>
    /// Returns a frozen <see cref="BitmapSource"/> rotated/flipped according to the
    /// EXIF orientation value (1–8). Returns the original if no transform is needed.
    /// </summary>
    private static BitmapSource ApplyExifOrientation(BitmapSource source, ushort orientation)
    {
        Transform? transform = orientation switch
        {
            2 => new ScaleTransform(-1, 1),
            3 => new RotateTransform(180),
            4 => new ScaleTransform(1, -1),
            5 => new TransformGroup { Children = { new RotateTransform(90), new ScaleTransform(-1, 1) } },
            6 => new RotateTransform(90),
            7 => new TransformGroup { Children = { new RotateTransform(270), new ScaleTransform(-1, 1) } },
            8 => new RotateTransform(270),
            _ => null
        };

        if (transform == null)
            return source;

        var transformed = new TransformedBitmap(source, transform);
        transformed.Freeze();
        return transformed;
    }

    /// <summary>
    /// Displays the currently selected image, resets zoom, and triggers prefetch.
    /// </summary>
    private async Task DisplayCurrentImageAsync()
    {
        var file = _navigator.Current;
        if (file == null)
        {
            ImageDisplay.Source = null;
            Title = "liv — Lightweight Image Viewer";
            return;
        }

        _zoom.Reset();
        ApplyTransform();

        var bmp = await _cache.GetAsync(file);
        ImageDisplay.Source = bmp;
        UpdateFittedDimensions();

        Title = $"liv — {Path.GetFileName(file)}";

        // Kick off background prefetch of neighbours
        _cache.PrefetchAround(file, offset => _navigator.PeekRelative(offset));
    }

    // =====================================================================
    //  Fitted-dimension helpers
    // =====================================================================

    /// <summary>
    /// Computes the rendered size of the image at zoom = 1.0 (Stretch=Uniform).
    /// </summary>
    private void UpdateFittedDimensions()
    {
        if (ImageDisplay.Source is not BitmapSource bmp) return;

        double viewW = RootGrid.ActualWidth;
        double viewH = RootGrid.ActualHeight;
        if (viewW <= 0 || viewH <= 0) return;

        double imgAspect = (double)bmp.PixelWidth / bmp.PixelHeight;
        double viewAspect = viewW / viewH;

        if (imgAspect > viewAspect)
        {
            _fittedWidth = viewW;
            _fittedHeight = viewW / imgAspect;
        }
        else
        {
            _fittedHeight = viewH;
            _fittedWidth = viewH * imgAspect;
        }
    }

    /// <summary>
    /// Pushes the current <see cref="ZoomController"/> state into the XAML render transforms.
    /// </summary>
    private void ApplyTransform()
    {
        ImageScale.ScaleX = _zoom.ZoomLevel;
        ImageScale.ScaleY = _zoom.ZoomLevel;
        ImageTranslate.X = _zoom.OffsetX;
        ImageTranslate.Y = _zoom.OffsetY;
    }

    // =====================================================================
    //  Navigation
    // =====================================================================

    private async Task NavigateAsync(bool forward)
    {
        if (forward)
            _navigator.MoveNext();
        else
            _navigator.MovePrevious();

        await DisplayCurrentImageAsync();
    }

    // =====================================================================
    //  Keyboard input
    // =====================================================================

    private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Right:
                await NavigateAsync(forward: true);
                e.Handled = true;
                break;

            case Key.Left:
                await NavigateAsync(forward: false);
                e.Handled = true;
                break;

            case Key.F11:
                ToggleFullScreen();
                e.Handled = true;
                break;

            case Key.Escape:
                if (_isFullScreen)
                    ToggleFullScreen();
                else
                    Close();
                e.Handled = true;
                break;

            case Key.Delete:
                await DeleteCurrentImageAsync();
                e.Handled = true;
                break;
        }
    }

    // =====================================================================
    //  Fullscreen
    // =====================================================================

    private void ToggleFullScreen()
    {
        if (_isFullScreen)
        {
            Topmost = _savedTopmost;
            WindowStyle = _savedWindowStyle;
            ResizeMode = _savedResizeMode;
            WindowState = _savedWindowState;
            _isFullScreen = false;
        }
        else
        {
            _savedWindowState = WindowState;
            _savedWindowStyle = WindowStyle;
            _savedResizeMode = ResizeMode;
            _savedTopmost = Topmost;

            // Go to Normal first so the maximized bounds are recalculated
            // with the new style, then use Topmost to cover the taskbar.
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
            Topmost = true;
            _isFullScreen = true;
        }
    }

    // =====================================================================
    //  Zoom (mouse wheel)
    // =====================================================================

    private void RootGrid_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_fittedWidth <= 0 || _fittedHeight <= 0) return;

        var pos = e.GetPosition(RootGrid);

        _zoom.ZoomToPoint(
            pos.X, pos.Y,
            RootGrid.ActualWidth, RootGrid.ActualHeight,
            _fittedWidth, _fittedHeight,
            zoomIn: e.Delta > 0);

        ApplyTransform();
        UpdateOverlayVisibility(pos);
        e.Handled = true;
    }

    // =====================================================================
    //  Pan / drag + overlay click detection
    // =====================================================================

    private async void RootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(RootGrid);

        // --- Double-click while zoomed → zoom fully out -------------------
        if (e.ClickCount == 2 && _zoom.IsZoomed)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Mouse.Capture(null);
            }
            _zoom.Reset();
            ApplyTransform();
            Cursor = Cursors.Arrow;
            UpdateOverlayVisibility(pos);
            e.Handled = true;
            return;
        }

        // --- Navigation-zone click (only when not zoomed) -----------------
        if (!_zoom.IsZoomed && _navigator.Count > 1)
        {
            double zoneWidth = RootGrid.ActualWidth / 5.0;

            if (pos.X < zoneWidth)
            {
                await NavigateAsync(forward: false);
                e.Handled = true;
                return;
            }
            if (pos.X > RootGrid.ActualWidth - zoneWidth)
            {
                await NavigateAsync(forward: true);
                e.Handled = true;
                return;
            }
        }

        // --- Start pan drag when zoomed -----------------------------------
        if (_zoom.IsZoomed)
        {
            _isDragging = true;
            _lastDragPoint = pos;
            Mouse.Capture(RootGrid);
            Cursor = Cursors.ScrollAll;
            e.Handled = true;
        }
    }

    private void RootGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            Mouse.Capture(null);
            Cursor = Cursors.Arrow;
            e.Handled = true;
        }
    }

    private void RootGrid_MouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(RootGrid);

        // Pan while dragging
        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            double dx = pos.X - _lastDragPoint.X;
            double dy = pos.Y - _lastDragPoint.Y;
            _zoom.Pan(dx, dy,
                RootGrid.ActualWidth, RootGrid.ActualHeight,
                _fittedWidth, _fittedHeight);
            ApplyTransform();
            _lastDragPoint = pos;
            return;
        }

        // Safety: release drag if the button was released outside our handler
        if (_isDragging && e.LeftButton == MouseButtonState.Released)
        {
            _isDragging = false;
            Mouse.Capture(null);
        }

        UpdateOverlayVisibility(pos);
    }

    // =====================================================================
    //  Navigation overlays
    // =====================================================================

    private void UpdateOverlayVisibility(Point mousePos)
    {
        if (_zoom.IsZoomed || _navigator.Count <= 1)
        {
            LeftNavZone.Visibility = Visibility.Collapsed;
            RightNavZone.Visibility = Visibility.Collapsed;
            if (!_isDragging) Cursor = Cursors.Arrow;
            return;
        }

        double width = RootGrid.ActualWidth;
        double zoneWidth = width / 5.0;
        bool inLeft = mousePos.X <= zoneWidth;
        bool inRight = mousePos.X >= width - zoneWidth;

        LeftNavZone.Visibility = inLeft ? Visibility.Visible : Visibility.Collapsed;
        RightNavZone.Visibility = inRight ? Visibility.Visible : Visibility.Collapsed;
        Cursor = (inLeft || inRight) ? Cursors.Hand : Cursors.Arrow;
    }

    // =====================================================================
    //  File deletion
    // =====================================================================

    private async Task DeleteCurrentImageAsync()
    {
        var file = _navigator.Current;
        if (file == null) return;

        try
        {
            _cache.Evict(file);
            _navigator.RemoveAndMoveNext(file);
            File.Delete(file);
            await DisplayCurrentImageAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not delete file:\n{ex.Message}",
                "liv",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    // =====================================================================
    //  Window events
    // =====================================================================

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        await DisplayCurrentImageAsync();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateFittedDimensions();
        if (_zoom.IsZoomed)
        {
            _zoom.ClampOffset(
                RootGrid.ActualWidth, RootGrid.ActualHeight,
                _fittedWidth, _fittedHeight);
            ApplyTransform();
        }
    }

    // =====================================================================
    //  FileSystemWatcher — keeps the file list in sync with the folder
    // =====================================================================

    private void SetupFileWatcher()
    {
        try
        {
            _watcher = new FileSystemWatcher(_folderPath)
            {
                NotifyFilter = NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            foreach (var ext in ImageFileScanner.GetSupportedExtensions())
                _watcher.Filters.Add($"*{ext}");

            _watcher.Created += OnFileSystemChanged;
            _watcher.Deleted += OnFileSystemChanged;
            _watcher.Renamed += (s, e) => OnFileSystemChanged(s, e);
        }
        catch
        {
            // If watching fails (e.g. network path), continue without live updates
        }
    }

    private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            var files = ImageFileScanner.ScanFolder(_folderPath);
            _navigator.UpdateFiles(files);
        });
    }

    // =====================================================================
    //  Cleanup
    // =====================================================================

    protected override void OnClosed(EventArgs e)
    {
        _watcher?.Dispose();
        _cache.Dispose();
        base.OnClosed(e);
    }
}