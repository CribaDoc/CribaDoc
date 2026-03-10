using System.Collections.Generic;

namespace CribaDoc.Core.Extractor
{
    /// Modelo limpio con los campos que necesita la UI
    public sealed class PaperVista
    {
        public string? Url { get; set; }

        public string? Doi { get; set; }

        public List<string> Titulos { get; set; } = new List<string>();

        public List<string> Autores { get; set; } = new List<string>();

        public List<string> Keywords { get; set; } = new List<string>();

        public string? Resumen { get; set; }

        public int? AnioPublicacion { get; set; }
    }
}
