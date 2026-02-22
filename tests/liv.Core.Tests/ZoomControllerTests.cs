using liv.Core;

namespace liv.Core.Tests;

public class ZoomControllerTests
{
    private const double Epsilon = 1e-6;

    // ---- Initial state --------------------------------------------------

    [Fact]
    public void Initial_ZoomLevel_IsOne()
    {
        var z = new ZoomController();

        Assert.Equal(1.0, z.ZoomLevel);
        Assert.Equal(0.0, z.OffsetX);
        Assert.Equal(0.0, z.OffsetY);
        Assert.False(z.IsZoomed);
    }

    // ---- ZoomToPoint (zoom in) ------------------------------------------

    [Fact]
    public void ZoomIn_IncreasesZoomLevel()
    {
        var z = new ZoomController();

        z.ZoomToPoint(500, 400, 1000, 800, 1000, 800, zoomIn: true);

        Assert.True(z.ZoomLevel > 1.0);
        Assert.True(z.IsZoomed);
    }

    [Fact]
    public void ZoomIn_AnchorPointStaysFixed()
    {
        var z = new ZoomController();
        double anchorX = 600, anchorY = 300;
        double viewW = 1000, viewH = 800;
        double imgW = 1000, imgH = 750;
        double centerX = viewW / 2.0, centerY = viewH / 2.0;

        // Compute image point under anchor before zoom
        double imgPtX = (anchorX - centerX - z.OffsetX) / z.ZoomLevel;
        double imgPtY = (anchorY - centerY - z.OffsetY) / z.ZoomLevel;

        z.ZoomToPoint(anchorX, anchorY, viewW, viewH, imgW, imgH, zoomIn: true);

        // Verify: the same image point maps back to the anchor
        double screenX = imgPtX * z.ZoomLevel + centerX + z.OffsetX;
        double screenY = imgPtY * z.ZoomLevel + centerY + z.OffsetY;

        Assert.InRange(screenX, anchorX - Epsilon, anchorX + Epsilon);
        Assert.InRange(screenY, anchorY - Epsilon, anchorY + Epsilon);
    }

    [Fact]
    public void ZoomIn_MultipleSteps_AnchorRemainsStable()
    {
        var z = new ZoomController();
        double ax = 700, ay = 200;
        double vw = 1920, vh = 1080;
        double iw = 1920, ih = 1080;
        double cx = vw / 2.0, cy = vh / 2.0;

        for (int i = 0; i < 10; i++)
        {
            double imgX = (ax - cx - z.OffsetX) / z.ZoomLevel;
            double imgY = (ay - cy - z.OffsetY) / z.ZoomLevel;

            z.ZoomToPoint(ax, ay, vw, vh, iw, ih, zoomIn: true);

            double sx = imgX * z.ZoomLevel + cx + z.OffsetX;
            double sy = imgY * z.ZoomLevel + cy + z.OffsetY;

            Assert.InRange(sx, ax - 1.0, ax + 1.0);
            Assert.InRange(sy, ay - 1.0, ay + 1.0);
        }
    }

    // ---- ZoomToPoint (zoom out) -----------------------------------------

    [Fact]
    public void ZoomOut_BelowOne_ClampsToFitAndResets()
    {
        var z = new ZoomController();

        z.ZoomToPoint(500, 400, 1000, 800, 1000, 800, zoomIn: false);

        Assert.Equal(1.0, z.ZoomLevel);
        Assert.Equal(0.0, z.OffsetX);
        Assert.Equal(0.0, z.OffsetY);
        Assert.False(z.IsZoomed);
    }

    [Fact]
    public void ZoomOut_FromZoomedIn_DecreasesZoom()
    {
        var z = new ZoomController();
        z.ZoomToPoint(500, 400, 1000, 800, 1000, 800, zoomIn: true);
        double afterZoomIn = z.ZoomLevel;

        z.ZoomToPoint(500, 400, 1000, 800, 1000, 800, zoomIn: false);

        Assert.True(z.ZoomLevel < afterZoomIn);
    }

    // ---- MaxZoom --------------------------------------------------------

    [Fact]
    public void ZoomIn_RespectsMaxZoom()
    {
        var z = new ZoomController { MaxZoom = 2.0 };

        for (int i = 0; i < 50; i++)
            z.ZoomToPoint(500, 400, 1000, 800, 1000, 800, zoomIn: true);

        Assert.True(z.ZoomLevel <= 2.0 + Epsilon);
    }

    // ---- Pan ------------------------------------------------------------

    [Fact]
    public void Pan_WhenNotZoomed_HasNoEffect()
    {
        var z = new ZoomController();

        z.Pan(100, 50, 1000, 800, 1000, 800);

        Assert.Equal(0.0, z.OffsetX);
        Assert.Equal(0.0, z.OffsetY);
    }

    [Fact]
    public void Pan_WhenZoomed_MovesOffset()
    {
        var z = new ZoomController();
        // Zoom in first
        for (int i = 0; i < 5; i++)
            z.ZoomToPoint(500, 400, 1000, 800, 1000, 800, zoomIn: true);

        double oldX = z.OffsetX;
        z.Pan(50, 30, 1000, 800, 1000, 800);

        // Offset should change (exact value depends on clamping)
        Assert.True(Math.Abs(z.OffsetX - oldX) > 0 || Math.Abs(z.OffsetY) > 0);
    }

    // ---- Reset ----------------------------------------------------------

    [Fact]
    public void Reset_RestoresInitialState()
    {
        var z = new ZoomController();
        z.ZoomToPoint(500, 400, 1000, 800, 1000, 800, zoomIn: true);
        z.Pan(10, 10, 1000, 800, 1000, 800);

        z.Reset();

        Assert.Equal(1.0, z.ZoomLevel);
        Assert.Equal(0.0, z.OffsetX);
        Assert.Equal(0.0, z.OffsetY);
        Assert.False(z.IsZoomed);
    }

    // ---- ClampOffset ----------------------------------------------------

    [Fact]
    public void ClampOffset_CentersWhenImageSmallerThanViewport()
    {
        var z = new ZoomController();

        // Manually set up a scenario via reflection or by zooming.
        // Use zoom to get a known state, then call ClampOffset.
        z.Reset();
        z.ClampOffset(1000, 800, 500, 400); // image smaller than viewport

        Assert.Equal(0.0, z.OffsetX);
        Assert.Equal(0.0, z.OffsetY);
    }

    // ---- ZoomStep property ----------------------------------------------

    [Fact]
    public void ZoomStep_AffectsZoomAmount()
    {
        var z1 = new ZoomController { ZoomStep = 1.1 };
        var z2 = new ZoomController { ZoomStep = 2.0 };

        z1.ZoomToPoint(500, 400, 1000, 800, 1000, 800, zoomIn: true);
        z2.ZoomToPoint(500, 400, 1000, 800, 1000, 800, zoomIn: true);

        Assert.True(z2.ZoomLevel > z1.ZoomLevel);
    }
}
