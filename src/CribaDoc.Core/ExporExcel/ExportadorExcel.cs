using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;

namespace CribaDoc.Core.ExporExcel
{
    public sealed class ExportadorExcel : IExportadorExcel
    {
        /// Exporta un Excel con 1 hoja por búsqueda.
        /// La clave del diccionario es el nombre de la hoja.
        public byte[] Exportar(Dictionary<string, List<FilaExcel>> hojas)
        {
            hojas ??= new Dictionary<string, List<FilaExcel>>();

            using var wb = new XLWorkbook();

            foreach (var kv in hojas)
            {
                var nombreHoja = LimpiarNombreHoja(kv.Key);
                var filas = kv.Value ?? new List<FilaExcel>();

                var ws = wb.Worksheets.Add(nombreHoja);

                // Cabecera
                ws.Cell(1, 1).Value = "BusquedaOrden";
                ws.Cell(1, 2).Value = "BusquedaNombre";
                ws.Cell(1, 3).Value = "DecisionTipo";
                ws.Cell(1, 4).Value = "Nota";
                ws.Cell(1, 5).Value = "CriteriosAplicados";
                ws.Cell(1, 6).Value = "Titulo";
                ws.Cell(1, 7).Value = "Anio";
                ws.Cell(1, 8).Value = "Doi";
                ws.Cell(1, 9).Value = "Url";

                // Filas
                var row = 2;
                foreach (var f in filas)
                {
                    ws.Cell(row, 1).Value = f.BusquedaOrden;
                    ws.Cell(row, 2).Value = f.BusquedaNombre;
                    ws.Cell(row, 3).Value = f.DecisionTipo;
                    ws.Cell(row, 4).Value = f.Nota;
                    ws.Cell(row, 5).Value = (f.CriteriosAplicados == null) ? null : string.Join("; ", f.CriteriosAplicados);
                    ws.Cell(row, 6).Value = f.Titulo;
                    ws.Cell(row, 7).Value = f.Anio;
                    ws.Cell(row, 8).Value = f.Doi;
                    ws.Cell(row, 9).Value = f.Url;

                    row++;
                }

                // Un poco de “calidad de vida”
                ws.Range(1, 1, 1, 9).Style.Font.Bold = true;
                ws.Columns().AdjustToContents();
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static string LimpiarNombreHoja(string? nombre)
        {
            // Excel limita a 31 caracteres y no permite ciertos caracteres
            var s = (nombre ?? "Hoja").Trim();
            if (s.Length == 0) s = "Hoja";

            var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            foreach (var c in invalid)
                s = s.Replace(c, '_');

            if (s.Length > 31)
                s = s.Substring(0, 31);

            return s;
        }
    }
}
