namespace liv.Core;

/// <summary>
/// Manages zoom level and pan offset for an image displayed in a viewport.
/// Supports anchor-point zooming: the image coordinate under the mouse cursor
/// stays fixed during zoom operations.
/// </summary>
public class ZoomController
{
    private double _zoomLevel = 1.0;
    private double _offsetX;
    private double _offsetY;

    /// <summary>
    /// Multiplier applied per mouse-wheel notch. Default is 1.15 (15 % per step).
    /// </summary>
    public double ZoomStep { get; set; } = 1.15;

    /// <summary>
    /// Maximum allowed zoom level. Default is 50×.
    /// </summary>
    public double MaxZoom { get; set; } = 50.0;

    /// <summary>Current zoom level (1.0 = fit-to-window).</summary>
    public double ZoomLevel => _zoomLevel;

    /// <summary>Horizontal translation offset (viewport pixels).</summary>
    public double OffsetX => _offsetX;

    /// <summary>Vertical translation offset (viewport pixels).</summary>
    public double OffsetY => _offsetY;

    /// <summary>Whether the image is currently zoomed beyond fit-to-window.</summary>
    public bool IsZoomed => _zoomLevel > 1.0 + 1e-9;

    /// <summary>
    /// Zooms towards or away from an anchor point so that the image coordinate
    /// under (<paramref name="anchorX"/>, <paramref name="anchorY"/>) remains
    /// visually stationary after the zoom.
    /// </summary>
    /// <param name="anchorX">Anchor X in viewport coordinates (0 = left edge).</param>
    /// <param name="anchorY">Anchor Y in viewport coordinates (0 = top edge).</param>
    /// <param name="viewWidth">Viewport width in pixels.</param>
    /// <param name="viewHeight">Viewport height in pixels.</param>
    /// <param name="imageWidth">Fitted image width at zoom 1.0.</param>
    /// <param name="imageHeight">Fitted image height at zoom 1.0.</param>
    /// <param name="zoomIn"><c>true</c> to zoom in, <c>false</c> to zoom out.</param>
    public void ZoomToPoint(double anchorX, double anchorY,
        double viewWidth, double viewHeight,
        double imageWidth, double imageHeight,
        bool zoomIn)
    {
        double oldZoom = _zoomLevel;
        double newZoom = zoomIn ? oldZoom * ZoomStep : oldZoom / ZoomStep;

        // Clamp to [1.0, MaxZoom]
        newZoom = Math.Max(1.0, Math.Min(MaxZoom, newZoom));
        if (Math.Abs(newZoom - oldZoom) < 1e-12)
            return;

        // Anchor-point math (coordinates relative to viewport center):
        //   imagePoint = (anchor - center - offset) / oldZoom
        //   newOffset  = anchor - center - imagePoint * newZoom
        //              = (anchor - center)(1 - ratio) + offset * ratio
        double centerX = viewWidth / 2.0;
        double centerY = viewHeight / 2.0;
        double ratio = newZoom / oldZoom;

        _offsetX = (anchorX - centerX) * (1.0 - ratio) + _offsetX * ratio;
        _offsetY = (anchorY - centerY) * (1.0 - ratio) + _offsetY * ratio;
        _zoomLevel = newZoom;

        // Snap back to fit when effectively at 1×
        if (newZoom <= 1.0 + 1e-9)
        {
            Reset();
            return;
        }

        ClampOffset(viewWidth, viewHeight, imageWidth, imageHeight);
    }

    /// <summary>
    /// Pans the image by (<paramref name="deltaX"/>, <paramref name="deltaY"/>) viewport pixels.
    /// Has no effect when the image is at fit-to-window zoom.
    /// </summary>
    public void Pan(double deltaX, double deltaY,
        double viewWidth, double viewHeight,
        double imageWidth, double imageHeight)
    {
        if (!IsZoomed) return;

        _offsetX += deltaX;
        _offsetY += deltaY;
        ClampOffset(viewWidth, viewHeight, imageWidth, imageHeight);
    }

    /// <summary>
    /// Resets zoom to fit-to-window (zoom = 1.0, offset = 0,0).
    /// </summary>
    public void Reset()
    {
        _zoomLevel = 1.0;
        _offsetX = 0;
        _offsetY = 0;
    }

    /// <summary>
    /// Clamps the pan offset so the image never pulls away from the viewport edges.
    /// When the scaled image is smaller than the viewport on an axis, it is centered.
    /// </summary>
    public void ClampOffset(double viewWidth, double viewHeight,
        double imageWidth, double imageHeight)
    {
        double scaledW = imageWidth * _zoomLevel;
        double scaledH = imageHeight * _zoomLevel;

        if (scaledW <= viewWidth)
        {
            _offsetX = 0; // center
        }
        else
        {
            double max = (scaledW - viewWidth) / 2.0;
            _offsetX = Math.Clamp(_offsetX, -max, max);
        }

        if (scaledH <= viewHeight)
        {
            _offsetY = 0; // center
        }
        else
        {
            double max = (scaledH - viewHeight) / 2.0;
            _offsetY = Math.Clamp(_offsetY, -max, max);
        }
    }
}
