using CadTool.Core.Math;

namespace CadTool.Core.Tests.Math;

public class BoundingBox3DTests
{
    [Fact]
    public void Constructor_NormalizesMinMax()
    {
        var bbox = new BoundingBox3D(new Vector3D(5, 5, 5), new Vector3D(0, 0, 0));
        Assert.Equal(new Vector3D(0, 0, 0), bbox.Min);
        Assert.Equal(new Vector3D(5, 5, 5), bbox.Max);
    }

    [Fact]
    public void Center_IsMiddlePoint()
    {
        var bbox = new BoundingBox3D(new Vector3D(0, 0, 0), new Vector3D(10, 10, 10));
        Assert.Equal(new Vector3D(5, 5, 5), bbox.Center);
    }

    [Fact]
    public void Size_ReturnsDimensions()
    {
        var bbox = new BoundingBox3D(new Vector3D(1, 2, 3), new Vector3D(4, 6, 8));
        Assert.Equal(new Vector3D(3, 4, 5), bbox.Size);
    }

    [Fact]
    public void Contains_InsidePoint_True()
    {
        var bbox = new BoundingBox3D(new Vector3D(0, 0, 0), new Vector3D(10, 10, 10));
        Assert.True(bbox.Contains(new Vector3D(5, 5, 5)));
    }

    [Fact]
    public void Contains_OutsidePoint_False()
    {
        var bbox = new BoundingBox3D(new Vector3D(0, 0, 0), new Vector3D(10, 10, 10));
        Assert.False(bbox.Contains(new Vector3D(15, 5, 5)));
    }

    [Fact]
    public void Contains_BoundaryPoint_True()
    {
        var bbox = new BoundingBox3D(new Vector3D(0, 0, 0), new Vector3D(10, 10, 10));
        Assert.True(bbox.Contains(new Vector3D(0, 0, 0)));
        Assert.True(bbox.Contains(new Vector3D(10, 10, 10)));
    }

    [Fact]
    public void Intersects_OverlappingBoxes_True()
    {
        var a = new BoundingBox3D(new Vector3D(0, 0, 0), new Vector3D(5, 5, 5));
        var b = new BoundingBox3D(new Vector3D(3, 3, 3), new Vector3D(8, 8, 8));
        Assert.True(a.Intersects(b));
    }

    [Fact]
    public void Intersects_NonOverlapping_False()
    {
        var a = new BoundingBox3D(new Vector3D(0, 0, 0), new Vector3D(1, 1, 1));
        var b = new BoundingBox3D(new Vector3D(5, 5, 5), new Vector3D(6, 6, 6));
        Assert.False(a.Intersects(b));
    }

    [Fact]
    public void Union_CombinesTwoBoxes()
    {
        var a = new BoundingBox3D(new Vector3D(0, 0, 0), new Vector3D(1, 1, 1));
        var b = new BoundingBox3D(new Vector3D(5, 5, 5), new Vector3D(6, 6, 6));
        var combined = a.Union(b);
        Assert.Equal(new Vector3D(0, 0, 0), combined.Min);
        Assert.Equal(new Vector3D(6, 6, 6), combined.Max);
    }
}
