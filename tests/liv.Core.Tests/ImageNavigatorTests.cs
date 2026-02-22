using liv.Core;

namespace liv.Core.Tests;

public class ImageNavigatorTests
{
    // ---- Constructor & Current ------------------------------------------

    [Fact]
    public void Constructor_WithFiles_SetsCurrentToStartIndex()
    {
        var files = new[] { "a.jpg", "b.jpg", "c.jpg" };
        var nav = new ImageNavigator(files, 1);

        Assert.Equal("b.jpg", nav.Current);
        Assert.Equal(1, nav.CurrentIndex);
        Assert.Equal(3, nav.Count);
    }

    [Fact]
    public void Constructor_EmptyList_CurrentIsNull()
    {
        var nav = new ImageNavigator(Array.Empty<string>(), 0);

        Assert.Null(nav.Current);
        Assert.Equal(0, nav.Count);
    }

    [Fact]
    public void Constructor_StartIndexOutOfRange_WrapsAround()
    {
        var files = new[] { "a.jpg", "b.jpg", "c.jpg" };
        var nav = new ImageNavigator(files, 5); // 5 % 3 = 2

        Assert.Equal("c.jpg", nav.Current);
    }

    // ---- MoveNext -------------------------------------------------------

    [Fact]
    public void MoveNext_AdvancesToNextFile()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg", "c.jpg" }, 0);

        Assert.Equal("b.jpg", nav.MoveNext());
        Assert.Equal(1, nav.CurrentIndex);
    }

    [Fact]
    public void MoveNext_AtLastFile_WrapsToFirst()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg", "c.jpg" }, 2);

        Assert.Equal("a.jpg", nav.MoveNext());
        Assert.Equal(0, nav.CurrentIndex);
    }

    [Fact]
    public void MoveNext_EmptyList_ReturnsNull()
    {
        var nav = new ImageNavigator(Array.Empty<string>(), 0);
        Assert.Null(nav.MoveNext());
    }

    [Fact]
    public void MoveNext_SingleFile_StaysOnSameFile()
    {
        var nav = new ImageNavigator(new[] { "only.jpg" }, 0);

        Assert.Equal("only.jpg", nav.MoveNext());
    }

    // ---- MovePrevious ---------------------------------------------------

    [Fact]
    public void MovePrevious_GoesBackOneFile()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg", "c.jpg" }, 2);

        Assert.Equal("b.jpg", nav.MovePrevious());
    }

    [Fact]
    public void MovePrevious_AtFirstFile_WrapsToLast()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg", "c.jpg" }, 0);

        Assert.Equal("c.jpg", nav.MovePrevious());
    }

    [Fact]
    public void MovePrevious_EmptyList_ReturnsNull()
    {
        var nav = new ImageNavigator(Array.Empty<string>(), 0);
        Assert.Null(nav.MovePrevious());
    }

    // ---- PeekRelative ---------------------------------------------------

    [Fact]
    public void PeekRelative_DoesNotMoveCursor()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg", "c.jpg" }, 1);

        Assert.Equal("c.jpg", nav.PeekRelative(1));
        Assert.Equal("a.jpg", nav.PeekRelative(-1));
        Assert.Equal("b.jpg", nav.Current); // unchanged
    }

    [Fact]
    public void PeekRelative_WrapsCircularly()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg", "c.jpg" }, 2);

        Assert.Equal("a.jpg", nav.PeekRelative(1));   // wraps forward
        Assert.Equal("b.jpg", nav.PeekRelative(-1));   // normal backward
    }

    [Fact]
    public void PeekRelative_ZeroOffset_ReturnsCurrent()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg" }, 0);
        Assert.Equal("a.jpg", nav.PeekRelative(0));
    }

    [Fact]
    public void PeekRelative_EmptyList_ReturnsNull()
    {
        var nav = new ImageNavigator(Array.Empty<string>(), 0);
        Assert.Null(nav.PeekRelative(1));
    }

    // ---- UpdateFiles ----------------------------------------------------

    [Fact]
    public void UpdateFiles_PreservesCurrentSelection()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg", "c.jpg" }, 1);
        nav.UpdateFiles(new[] { "a.jpg", "b.jpg", "c.jpg", "d.jpg" });

        Assert.Equal("b.jpg", nav.Current);
        Assert.Equal(4, nav.Count);
    }

    [Fact]
    public void UpdateFiles_CurrentRemoved_ClampsIndex()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg", "c.jpg" }, 1);
        nav.UpdateFiles(new[] { "a.jpg", "c.jpg" }); // "b.jpg" removed

        // Should fall back to index 1 → "c.jpg"
        Assert.NotNull(nav.Current);
        Assert.Equal(2, nav.Count);
    }

    [Fact]
    public void UpdateFiles_EmptyList_CurrentBecomesNull()
    {
        var nav = new ImageNavigator(new[] { "a.jpg" }, 0);
        nav.UpdateFiles(Array.Empty<string>());

        Assert.Null(nav.Current);
        Assert.Equal(0, nav.Count);
    }

    // ---- RemoveAndMoveNext ----------------------------------------------

    [Fact]
    public void RemoveAndMoveNext_RemovesCurrentAndAdvances()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg", "c.jpg" }, 1);

        var next = nav.RemoveAndMoveNext("b.jpg");

        Assert.Equal("c.jpg", next);
        Assert.Equal(2, nav.Count);
    }

    [Fact]
    public void RemoveAndMoveNext_LastItem_WrapsToFirst()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg", "c.jpg" }, 2);

        var next = nav.RemoveAndMoveNext("c.jpg");

        Assert.Equal("a.jpg", next);
    }

    [Fact]
    public void RemoveAndMoveNext_OnlyFile_ReturnsNull()
    {
        var nav = new ImageNavigator(new[] { "only.jpg" }, 0);

        var next = nav.RemoveAndMoveNext("only.jpg");

        Assert.Null(next);
        Assert.Equal(0, nav.Count);
    }

    [Fact]
    public void RemoveAndMoveNext_FileBefore_AdjustsIndex()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg", "c.jpg" }, 2);

        var result = nav.RemoveAndMoveNext("a.jpg");

        Assert.Equal("c.jpg", result); // current was c, still c after removing a
        Assert.Equal(1, nav.CurrentIndex);
    }

    [Fact]
    public void RemoveAndMoveNext_UnknownFile_NoChange()
    {
        var nav = new ImageNavigator(new[] { "a.jpg", "b.jpg" }, 0);

        var result = nav.RemoveAndMoveNext("unknown.jpg");

        Assert.Equal("a.jpg", result);
        Assert.Equal(2, nav.Count);
    }

    // ---- WrapIndex (static) ---------------------------------------------

    [Theory]
    [InlineData(0, 3, 0)]
    [InlineData(2, 3, 2)]
    [InlineData(3, 3, 0)]   // wrap forward
    [InlineData(-1, 3, 2)]  // wrap backward
    [InlineData(-3, 3, 0)]  // full wrap backward
    [InlineData(7, 3, 1)]   // multiple wraps
    [InlineData(0, 1, 0)]   // single element
    public void WrapIndex_ReturnsExpected(int index, int count, int expected)
    {
        Assert.Equal(expected, ImageNavigator.WrapIndex(index, count));
    }

    [Fact]
    public void WrapIndex_CountZero_ReturnsZero()
    {
        Assert.Equal(0, ImageNavigator.WrapIndex(5, 0));
    }
}
