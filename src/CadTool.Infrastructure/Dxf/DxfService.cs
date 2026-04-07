using CadTool.Core.Domain;
using CadTool.Core.Interfaces;
using CadTool.Core.Math;
using CadTool.Core.Mesh;
using CadTool.Core.Primitives;
using netDxf;
using netDxf.Entities;

namespace CadTool.Infrastructure.Dxf;

/// <summary>
/// DXF Im/Export-Service basierend auf netDxf.
/// Unterstuetzt Import von 3DFACE-Entitaeten und Polylines sowie
/// Export von Primitiven und Meshes als 3DFACE-Entitaeten.
/// </summary>
public sealed class DxfService : IDxfService
{
    /// <inheritdoc />
    public IReadOnlyList<CadBody> Import(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Dateipfad darf nicht leer sein.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("DXF-Datei nicht gefunden.", filePath);

        var dxfDoc = DxfDocument.Load(filePath);
        if (dxfDoc is null)
            throw new InvalidOperationException($"DXF-Datei konnte nicht geladen werden: {filePath}");

        var bodies = new List<CadBody>();

        // 3DFACE-Entitaeten importieren
        var faces = dxfDoc.Entities.Faces3D.ToList();
        if (faces.Count > 0)
        {
            var mesh = new TriangleMesh();
            foreach (var face in faces)
            {
                var v0 = ConvertToVector3D(face.FirstVertex);
                var v1 = ConvertToVector3D(face.SecondVertex);
                var v2 = ConvertToVector3D(face.ThirdVertex);
                var v3 = ConvertToVector3D(face.FourthVertex);

                var i0 = mesh.AddVertex(v0);
                var i1 = mesh.AddVertex(v1);
                var i2 = mesh.AddVertex(v2);
                mesh.AddTriangle(i0, i1, i2);

                // Wenn der vierte Punkt vom dritten abweicht, ist es ein Quad (zwei Dreiecke)
                if (v2 != v3)
                {
                    var i3 = mesh.AddVertex(v3);
                    mesh.AddTriangle(i0, i2, i3);
                }
            }

            if (mesh.TriangleCount > 0)
            {
                bodies.Add(new CadBody("DXF-Import (3DFaces)", Matrix4x4.Identity, mesh));
            }
        }

        // Polylinien als separate Koerper (als offene Meshes)
        var polylines = dxfDoc.Entities.Polylines3D;
        var polylineIndex = 0;
        foreach (var polyline in polylines)
        {
            var vertices = polyline.Vertexes;
            if (vertices.Count < 2) continue;

            var mesh = new TriangleMesh();
            foreach (var vertex in vertices)
            {
                mesh.AddVertex(new Vector3D(vertex.X, vertex.Y, vertex.Z));
            }

            // Polyline als Linien-Mesh (keine Dreiecke, nur Vertices)
            bodies.Add(new CadBody($"Polyline-{polylineIndex++}", Matrix4x4.Identity, mesh));
        }

        return bodies.AsReadOnly();
    }

    /// <inheritdoc />
    public void Export(IReadOnlyList<CadBody> bodies, string filePath)
    {
        ArgumentNullException.ThrowIfNull(bodies);
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Dateipfad darf nicht leer sein.", nameof(filePath));

        var dxfDoc = new DxfDocument();

        foreach (var body in bodies)
        {
            if (!body.IsVisible) continue;

            if (body.Mesh is { } mesh)
            {
                ExportMesh(dxfDoc, mesh);
            }
            else if (body.Primitive is { } primitive)
            {
                ExportPrimitiveAsBoundingBox(dxfDoc, primitive);
            }
        }

        dxfDoc.Save(filePath);
    }

    private static void ExportMesh(DxfDocument doc, TriangleMesh mesh)
    {
        for (var i = 0; i < mesh.TriangleCount; i++)
        {
            var tri = mesh.GetTriangle(i);
            var face = new Face3D(
                ConvertToVector3(tri.V0),
                ConvertToVector3(tri.V1),
                ConvertToVector3(tri.V2));
            doc.Entities.Add(face);
        }
    }

    private static void ExportPrimitiveAsBoundingBox(DxfDocument doc, IPrimitive3D primitive)
    {
        // Primitive ohne Mesh werden als BoundingBox-Wireframe exportiert
        var bbox = primitive.GetBoundingBox();
        var min = bbox.Min;
        var max = bbox.Max;

        // 12 Kanten der Bounding Box als Lines
        AddLine(doc, min, new Vector3D(max.X, min.Y, min.Z));
        AddLine(doc, min, new Vector3D(min.X, max.Y, min.Z));
        AddLine(doc, min, new Vector3D(min.X, min.Y, max.Z));
        AddLine(doc, max, new Vector3D(min.X, max.Y, max.Z));
        AddLine(doc, max, new Vector3D(max.X, min.Y, max.Z));
        AddLine(doc, max, new Vector3D(max.X, max.Y, min.Z));

        AddLine(doc, new Vector3D(max.X, min.Y, min.Z), new Vector3D(max.X, max.Y, min.Z));
        AddLine(doc, new Vector3D(max.X, min.Y, min.Z), new Vector3D(max.X, min.Y, max.Z));
        AddLine(doc, new Vector3D(min.X, max.Y, min.Z), new Vector3D(max.X, max.Y, min.Z));
        AddLine(doc, new Vector3D(min.X, max.Y, min.Z), new Vector3D(min.X, max.Y, max.Z));
        AddLine(doc, new Vector3D(min.X, min.Y, max.Z), new Vector3D(max.X, min.Y, max.Z));
        AddLine(doc, new Vector3D(min.X, min.Y, max.Z), new Vector3D(min.X, max.Y, max.Z));
    }

    private static void AddLine(DxfDocument doc, Vector3D from, Vector3D to)
    {
        doc.Entities.Add(new netDxf.Entities.Line(ConvertToVector3(from), ConvertToVector3(to)));
    }

    private static Vector3D ConvertToVector3D(Vector3 v) => new(v.X, v.Y, v.Z);
    private static Vector3 ConvertToVector3(Vector3D v) => new(v.X, v.Y, v.Z);
}
