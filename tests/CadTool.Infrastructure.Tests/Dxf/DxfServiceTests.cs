using CadTool.Core.Domain;
using CadTool.Core.Math;
using CadTool.Core.Mesh;
using CadTool.Core.Primitives;
using CadTool.Infrastructure.Dxf;

namespace CadTool.Infrastructure.Tests.Dxf;

public class DxfServiceTests
{
    private readonly DxfService _sut = new();
    private readonly string _tempDir;

    public DxfServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "CadToolTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Export_CreatesFile()
    {
        var mesh = CreateSimpleMesh();
        var body = new CadBody("TestBody", Matrix4x4.Identity, mesh);
        var filePath = Path.Combine(_tempDir, "test_export.dxf");

        _sut.Export([body], filePath);

        Assert.True(File.Exists(filePath));
        Assert.True(new FileInfo(filePath).Length > 0);
    }

    [Fact]
    public void Export_Import_Roundtrip()
    {
        var mesh = CreateSimpleMesh();
        var body = new CadBody("Roundtrip", Matrix4x4.Identity, mesh);
        var filePath = Path.Combine(_tempDir, "roundtrip.dxf");

        _sut.Export([body], filePath);
        var imported = _sut.Import(filePath);

        Assert.NotEmpty(imported);
        var importedBody = imported[0];
        Assert.NotNull(importedBody.Mesh);
        Assert.Equal(mesh.TriangleCount, importedBody.Mesh.TriangleCount);
    }

    [Fact]
    public void Export_EmptyList_CreatesValidFile()
    {
        var filePath = Path.Combine(_tempDir, "empty.dxf");
        _sut.Export([], filePath);
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void Export_InvisibleBody_Skipped()
    {
        var mesh = CreateSimpleMesh();
        var body = new CadBody("Hidden", Matrix4x4.Identity, mesh) { IsVisible = false };
        var filePath = Path.Combine(_tempDir, "invisible.dxf");

        _sut.Export([body], filePath);

        var imported = _sut.Import(filePath);
        Assert.Empty(imported);
    }

    [Fact]
    public void Import_NonExistentFile_Throws()
    {
        Assert.Throws<FileNotFoundException>(() =>
            _sut.Import("/nonexistent/path/test.dxf"));
    }

    [Fact]
    public void Import_EmptyPath_Throws()
    {
        Assert.Throws<ArgumentException>(() => _sut.Import(""));
        Assert.Throws<ArgumentException>(() => _sut.Import("  "));
    }

    [Fact]
    public void Export_NullBodies_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Export(null!, "test.dxf"));
    }

    [Fact]
    public void Export_EmptyPath_Throws()
    {
        Assert.Throws<ArgumentException>(() => _sut.Export([], ""));
    }

    [Fact]
    public void Export_PrimitiveBody_ExportsAsWireframe()
    {
        var box = new BoxPrimitive(5, 5, 5);
        var body = new CadBody("BoxWireframe", box);
        var filePath = Path.Combine(_tempDir, "wireframe.dxf");

        _sut.Export([body], filePath);

        Assert.True(File.Exists(filePath));
        Assert.True(new FileInfo(filePath).Length > 0);
    }

    private static TriangleMesh CreateSimpleMesh()
    {
        var mesh = new TriangleMesh();
        mesh.AddVertex(new Vector3D(0, 0, 0));
        mesh.AddVertex(new Vector3D(1, 0, 0));
        mesh.AddVertex(new Vector3D(0, 1, 0));
        mesh.AddVertex(new Vector3D(0, 0, 1));
        mesh.AddTriangle(0, 1, 2);
        mesh.AddTriangle(0, 2, 3);
        mesh.AddTriangle(0, 3, 1);
        mesh.AddTriangle(1, 3, 2);
        return mesh;
    }
}
