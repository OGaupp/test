using CadTool.Core.Domain;
using CadTool.Core.Mesh;
using CadTool.Core.Primitives;
using CadTool.Geometry.Mesh;
using HelixToolkit.SharpDX.Core;
using SharpDX;

namespace CadTool.WinUI.Converters;

/// <summary>
/// Konvertiert CadTool-Typen in HelixToolkit-kompatible Geometrie.
/// Bruecke zwischen Domain-Layer (CadTool.Core) und Rendering-Layer (HelixToolkit).
/// </summary>
public static class MeshConverter
{
    /// <summary>
    /// Konvertiert ein CadTool TriangleMesh in ein HelixToolkit MeshGeometry3D.
    /// </summary>
    public static MeshGeometry3D ToHelixMesh(TriangleMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        var positions = new Vector3Collection(mesh.VertexCount);
        var normals = new Vector3Collection(mesh.VertexCount);
        var indices = new IntCollection(mesh.TriangleCount * 3);

        // Vertices uebernehmen
        foreach (var vertex in mesh.Vertices)
        {
            positions.Add(new Vector3((float)vertex.X, (float)vertex.Y, (float)vertex.Z));
        }

        // Indizes + Normalen berechnen
        foreach (var (i0, i1, i2) in mesh.TriangleIndices)
        {
            indices.Add(i0);
            indices.Add(i1);
            indices.Add(i2);
        }

        // Flat Normals berechnen (per-Vertex, gemittelt)
        var normalAccum = new Vector3[mesh.VertexCount];
        for (var i = 0; i < mesh.TriangleCount; i++)
        {
            var tri = mesh.GetTriangle(i);
            var v0 = mesh.Vertices[mesh.TriangleIndices[i].I0];
            var v1 = mesh.Vertices[mesh.TriangleIndices[i].I1];
            var v2 = mesh.Vertices[mesh.TriangleIndices[i].I2];

            var edge1 = ToSharpDX(v1) - ToSharpDX(v0);
            var edge2 = ToSharpDX(v2) - ToSharpDX(v0);
            var normal = Vector3.Cross(edge1, edge2);

            normalAccum[mesh.TriangleIndices[i].I0] += normal;
            normalAccum[mesh.TriangleIndices[i].I1] += normal;
            normalAccum[mesh.TriangleIndices[i].I2] += normal;
        }

        foreach (var n in normalAccum)
        {
            var normalized = n.LengthSquared() > 1e-10f ? Vector3.Normalize(n) : Vector3.UnitZ;
            normals.Add(normalized);
        }

        return new MeshGeometry3D
        {
            Positions = positions,
            Normals = normals,
            Indices = indices
        };
    }

    /// <summary>
    /// Konvertiert einen CadBody in ein HelixToolkit MeshGeometry3D.
    /// Erzeugt bei Bedarf ein Mesh aus dem Primitiv.
    /// </summary>
    public static MeshGeometry3D? ToHelixMesh(CadBody body)
    {
        ArgumentNullException.ThrowIfNull(body);

        if (body.Mesh is { } mesh)
        {
            return ToHelixMesh(mesh);
        }

        if (body.Primitive is { } primitive)
        {
            var generatedMesh = MeshGenerator.GenerateMesh(primitive);
            return ToHelixMesh(generatedMesh);
        }

        return null;
    }

    private static Vector3 ToSharpDX(CadTool.Core.Math.Vector3D v) =>
        new((float)v.X, (float)v.Y, (float)v.Z);
}
