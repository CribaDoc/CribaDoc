using System.Collections.Generic;

namespace CribaDoc.Core.ExporExcel
{
    /// Representa una fila del Excel resumen
    public sealed class FilaExcel
    {
        public int? BusquedaOrden { get; set; }

        public string? BusquedaNombre { get; set; }

        public string? DecisionTipo { get; set; }   // Result / Deleted

        public string? Nota { get; set; }

        public List<string> CriteriosAplicados { get; set; } = new List<string>();

        public string? Titulo { get; set; }

        public int? Anio { get; set; }

        public string? Doi { get; set; }

        public string? Url { get; set; }
    }
}
