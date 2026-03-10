using System.Collections.Generic;

namespace CribaDoc.Core.ExporExcel
{
    public interface IExportadorExcel
    {
        byte[] Exportar(Dictionary<string, List<FilaExcel>> hojas);
    }
}
