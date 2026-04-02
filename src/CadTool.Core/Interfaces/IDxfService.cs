using CadTool.Core.Domain;

namespace CadTool.Core.Interfaces;

/// <summary>
/// Service fuer DXF-Import und -Export.
/// Entkoppelt die konkrete DXF-Bibliothek vom Domain-Modell.
/// </summary>
public interface IDxfService
{
    /// <summary>
    /// Importiert Koerper aus einer DXF-Datei.
    /// </summary>
    /// <param name="filePath">Pfad zur DXF-Datei.</param>
    /// <returns>Liste importierter Koerper.</returns>
    IReadOnlyList<CadBody> Import(string filePath);

    /// <summary>
    /// Exportiert Koerper in eine DXF-Datei.
    /// </summary>
    /// <param name="bodies">Zu exportierende Koerper.</param>
    /// <param name="filePath">Zielpfad der DXF-Datei.</param>
    void Export(IReadOnlyList<CadBody> bodies, string filePath);
}
