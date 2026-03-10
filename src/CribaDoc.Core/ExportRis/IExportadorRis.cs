using System.Collections.Generic;

namespace CribaDoc.Core.ExportRis
{
    public interface IExportadorRis
    {
        string Exportar(IEnumerable<string> bloques);
    }
}