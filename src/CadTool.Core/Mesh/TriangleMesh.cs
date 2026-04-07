using CadTool.Core.Math;

namespace CadTool.Core.Mesh;

/// <summary>
/// Dreiecksnetz (Triangle Mesh) zur Repraesentation von 3D-Koerpern.
/// Vertices werden in einer gemeinsamen Liste gespeichert, Dreiecke referenzieren per Index.
/// </summary>
public sealed class TriangleMesh
{
    private readonly List<Vector3D> _vertices = [];
    private readonly List<(int I0, int I1, int I2)> _triangleIndices = [];

    /// <summary>Alle Vertices des Netzes.</summary>
    public IReadOnlyList<Vector3D> Vertices => _vertices.AsReadOnly();

    /// <summary>Alle Dreiecks-Indizes (je drei Indizes in die Vertex-Liste).</summary>
    public IReadOnlyList<(int I0, int I1, int I2)> TriangleIndices => _triangleIndices.AsReadOnly();

    /// <summary>Anzahl der Dreiecke.</summary>
    public int TriangleCount => _triangleIndices.Count;

    /// <summary>Anzahl der Vertices.</summary>
    public int VertexCount => _vertices.Count;

    /// <summary>Fuegt einen Vertex hinzu und gibt dessen Index zurueck.</summary>
    public int AddVertex(Vector3D vertex)
    {
        _vertices.Add(vertex);
        return _vertices.Count - 1;
    }

    /// <summary>Fuegt ein Dreieck ueber drei Vertex-Indizes hinzu.</summary>
    public void AddTriangle(int i0, int i1, int i2)
    {
        ValidateIndex(i0);
        ValidateIndex(i1);
        ValidateIndex(i2);
        _triangleIndices.Add((i0, i1, i2));
    }

    /// <summary>Gibt das Dreieck an der gegebenen Position zurueck.</summary>
    public Triangle3D GetTriangle(int index)
    {
        var (i0, i1, i2) = _triangleIndices[index];
        return new Triangle3D(_vertices[i0], _vertices[i1], _vertices[i2]);
    }

    /// <summary>Berechnet die Bounding Box des gesamten Netzes.</summary>
    public BoundingBox3D GetBoundingBox()
    {
        if (_vertices.Count == 0)
            return new BoundingBox3D(Vector3D.Zero, Vector3D.Zero);

        var min = _vertices[0];
        var max = _vertices[0];
        for (var i = 1; i < _vertices.Count; i++)
        {
            min = new Vector3D(
                System.Math.Min(min.X, _vertices[i].X),
                System.Math.Min(min.Y, _vertices[i].Y),
                System.Math.Min(min.Z, _vertices[i].Z));
            max = new Vector3D(
                System.Math.Max(max.X, _vertices[i].X),
                System.Math.Max(max.Y, _vertices[i].Y),
                System.Math.Max(max.Z, _vertices[i].Z));
        }

        return new BoundingBox3D(min, max);
    }

    /// <summary>Berechnet das Gesamtvolumen des Netzes (Divergenzsatz, setzt geschlossenes Netz voraus).</summary>
    public double CalculateVolume()
    {
        var volume = 0.0;
        foreach (var (i0, i1, i2) in _triangleIndices)
        {
            var v0 = _vertices[i0];
            var v1 = _vertices[i1];
            var v2 = _vertices[i2];
            volume += v0.Dot(v1.Cross(v2));
        }
        return System.Math.Abs(volume) / 6.0;
    }

    /// <summary>Erstellt eine tiefe Kopie des Netzes.</summary>
    public TriangleMesh Clone()
    {
        var clone = new TriangleMesh();
        foreach (var v in _vertices) clone.AddVertex(v);
        foreach (var (i0, i1, i2) in _triangleIndices) clone._triangleIndices.Add((i0, i1, i2));
        return clone;
    }

    /// <summary>Transformiert alle Vertices mit der gegebenen Matrix.</summary>
    public void Transform(Matrix4x4 transform)
    {
        for (var i = 0; i < _vertices.Count; i++)
        {
            _vertices[i] = transform.TransformPoint(_vertices[i]);
        }
    }

    /// <summary>Kehrt die Windungsordnung aller Dreiecke um (Normalen invertieren).</summary>
    public void InvertNormals()
    {
        for (var i = 0; i < _triangleIndices.Count; i++)
        {
            var (i0, i1, i2) = _triangleIndices[i];
            _triangleIndices[i] = (i0, i2, i1);
        }
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= _vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Vertex-Index {index} ausserhalb des gueltigen Bereichs [0, {_vertices.Count - 1}].");
    }
}
