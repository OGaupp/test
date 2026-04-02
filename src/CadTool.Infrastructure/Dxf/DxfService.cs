using CadTool.Core.Domain;
using CadTool.Core.Interfaces;

namespace CadTool.Infrastructure.Dxf;

/// <summary>
/// DXF Im/Export-Service.
/// 
/// Aktueller Stand: Stub-Implementierung.
/// Die vollstaendige Implementierung erfolgt nach Integration der netDxf-Bibliothek.
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

        // TODO: Implementierung mit netDxf in Phase 2
        throw new NotImplementedException(
            "DXF-Import ist noch nicht implementiert. Integration von netDxf steht aus.");
    }

    /// <inheritdoc />
    public void Export(IReadOnlyList<CadBody> bodies, string filePath)
    {
        ArgumentNullException.ThrowIfNull(bodies);
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Dateipfad darf nicht leer sein.", nameof(filePath));

        // TODO: Implementierung mit netDxf in Phase 2
        throw new NotImplementedException(
            "DXF-Export ist noch nicht implementiert. Integration von netDxf steht aus.");
    }
}
