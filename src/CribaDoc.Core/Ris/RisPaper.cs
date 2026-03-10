using System;
using System.Collections.Generic;
using System.Linq;
using CribaDoc.Core.Extractor;

namespace CribaDoc.Core.Ris
{
    /// Representa un paper RIS completo (bloque TY..ER)
    public sealed class RisPaper
    {
        public string TxtOr { get; }
        public List<RisTag> Tags { get; }

        public RisPaper(string txtOr, List<RisTag> tags)
        {
            TxtOr = txtOr ?? string.Empty;
            Tags = tags != null ? new List<RisTag>(tags) : new List<RisTag>();
        }

        public List<string> GetAll(string tag)
        {
            return Tags
                .Where(t => string.Equals(t.Tag, tag, StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Value)
                .ToList();
        }

        public string? GetFirst(params string[] tags)
        {
            foreach (var tag in tags)
            {
                var value = Tags
                    .FirstOrDefault(t => string.Equals(t.Tag, tag, StringComparison.OrdinalIgnoreCase))
                    ?.Value;

                if (value != null)
                    return value;
            }

            return null;
        }

        /// Convierte este RIS paper a un modelo "limpio" para la UI (PaperVista)
        public PaperVista ToVista()
        {
            var vista = new PaperVista();

            // URL: UR (fallback: LK)
            vista.Url = GetFirst("UR", "LK");

            // DOI: DO (fallback opcional: DI)
            vista.Doi = GetFirst("DO", "DI");

            // Título: TI o T1
            var titulo = GetFirst("TI", "T1");
            if (!string.IsNullOrWhiteSpace(titulo))
                vista.Titulos.Add(titulo);

            // Autores / Keywords
            vista.Autores = GetAll("AU");
            vista.Keywords = GetAll("KW");

            // Abstract
            vista.Resumen = GetFirst("AB");

            // Año: PY
            vista.AnioPublicacion = ParseAño(GetFirst("PY"));

            return vista;
        }

        private static int? ParseAño(string? py)
        {
            if (string.IsNullOrWhiteSpace(py))
                return null;

            var s = py.Trim();

            if (s.Length >= 4 && int.TryParse(s.Substring(0, 4), out var year))
                return year;

            return null;
        }
    }
}