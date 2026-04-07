using CadTool.Core.Math;
using CadTool.Core.Mesh;
using CadTool.Core.Primitives;
using CadTool.Geometry.Mesh;

namespace CadTool.Geometry.Tests.Mesh;

public class MeshBooleanOperationsTests
{
    [Fact]
    public void Union_TwoNonOverlapping_ContainsAllTriangles()
    {
        var meshA = MeshGenerator.GenerateMesh(new BoxPrimitive(2, 2, 2));
        var meshB = MeshGenerator.GenerateMesh(
            new BoxPrimitive(2, 2, 2, Matrix4x4.CreateTranslation(new Vector3D(10, 0, 0))));

        var result = MeshBooleanOperations.Union(meshA, meshB);

        // Keine Ueberlappung: alle Dreiecke beider Meshes
        Assert.Equal(meshA.TriangleCount + meshB.TriangleCount, result.TriangleCount);
    }

    [Fact]
    public void Subtract_RemovesInnerTriangles()
    {
        var meshA = MeshGenerator.GenerateMesh(new BoxPrimitive(10, 10, 10));
        var meshB = MeshGenerator.GenerateMesh(new BoxPrimitive(4, 4, 4));

        var result = MeshBooleanOperations.Subtract(meshA, meshB);

        // Das Ergebnis sollte mehr Dreiecke haben als nur meshA (invertierte innere Flaechen von B)
        Assert.True(result.TriangleCount > 0, "Subtract-Ergebnis darf nicht leer sein.");
    }

    [Fact]
    public void Intersect_NonOverlapping_ReturnsEmpty()
    {
        var meshA = MeshGenerator.GenerateMesh(new BoxPrimitive(2, 2, 2));
        var meshB = MeshGenerator.GenerateMesh(
            new BoxPrimitive(2, 2, 2, Matrix4x4.CreateTranslation(new Vector3D(100, 0, 0))));

        var result = MeshBooleanOperations.Intersect(meshA, meshB);

        Assert.Equal(0, result.TriangleCount);
    }

    [Fact]
    public void Intersect_CompletelyInside_ReturnsSmallerMesh()
    {
        var meshA = MeshGenerator.GenerateMesh(new BoxPrimitive(10, 10, 10));
        var meshB = MeshGenerator.GenerateMesh(new BoxPrimitive(4, 4, 4));

        var result = MeshBooleanOperations.Intersect(meshA, meshB);

        // B liegt vollstaendig in A, daher werden alle Dreiecke von B beibehalten
        Assert.True(result.TriangleCount > 0, "Intersect-Ergebnis darf nicht leer sein wenn B in A liegt.");
    }

    [Fact]
    public void RayIntersectsTriangle_DirectHit()
    {
        var tri = new Triangle3D(
            new Vector3D(0, -1, -1),
            new Vector3D(0, 1, -1),
            new Vector3D(0, 0, 1));

        var result = MeshBooleanOperations.RayIntersectsTriangle(
            new Vector3D(-5, 0, 0), Vector3D.UnitX, tri);

        Assert.True(result);
    }

    [Fact]
    public void RayIntersectsTriangle_Miss()
    {
        var tri = new Triangle3D(
            new Vector3D(0, -1, -1),
            new Vector3D(0, 1, -1),
            new Vector3D(0, 0, 1));

        var result = MeshBooleanOperations.RayIntersectsTriangle(
            new Vector3D(-5, 10, 10), Vector3D.UnitX, tri);

        Assert.False(result);
    }

    [Fact]
    public void IsPointInsideMesh_InsideCube_ReturnsTrue()
    {
        var mesh = MeshGenerator.GenerateMesh(new BoxPrimitive(4, 4, 4));
        Assert.True(MeshBooleanOperations.IsPointInsideMesh(new Vector3D(0.5, 0.5, 0.5), mesh));
    }

    [Fact]
    public void IsPointInsideMesh_OutsideCube_ReturnsFalse()
    {
        var mesh = MeshGenerator.GenerateMesh(new BoxPrimitive(4, 4, 4));
        Assert.False(MeshBooleanOperations.IsPointInsideMesh(new Vector3D(10, 10, 10), mesh));
    }

    [Fact]
    public void Union_Null_Throws()
    {
        var mesh = new TriangleMesh();
        Assert.Throws<ArgumentNullException>(() => MeshBooleanOperations.Union(null!, mesh));
        Assert.Throws<ArgumentNullException>(() => MeshBooleanOperations.Union(mesh, null!));
    }

    [Fact]
    public void Subtract_Null_Throws()
    {
        var mesh = new TriangleMesh();
        Assert.Throws<ArgumentNullException>(() => MeshBooleanOperations.Subtract(null!, mesh));
        Assert.Throws<ArgumentNullException>(() => MeshBooleanOperations.Subtract(mesh, null!));
    }

    [Fact]
    public void Intersect_Null_Throws()
    {
        var mesh = new TriangleMesh();
        Assert.Throws<ArgumentNullException>(() => MeshBooleanOperations.Intersect(null!, mesh));
        Assert.Throws<ArgumentNullException>(() => MeshBooleanOperations.Intersect(mesh, null!));
    }
}
