using System;
using System.Collections.Generic;
using System.Text;

namespace CribaDoc.Core.ExportRis
{
    /// Concatena bloques RIS (TY..ER) en un RIS final sin regenerar etiquetas.
    public sealed class ExportadorRis : IExportadorRis
    {
        public string Exportar(IEnumerable<string> bloques)
        {
            if (bloques == null)
                return string.Empty;

            var sb = new StringBuilder();
            var first = true;

            foreach (var b in bloques)
            {
                var block = (b ?? string.Empty).Trim();
                if (block.Length == 0)
                    continue;

                // Separación entre bloques: una línea en blanco (solo si ya hay contenido)
                if (!first)
                    sb.Append('\n');

                sb.Append(NormalizarBloque(block));
                first = false;
            }

            return sb.ToString();
        }

        private static string NormalizarBloque(string block)
        {
            // Normaliza saltos a '\n'
            var s = block.Replace("\r\n", "\n").Replace("\r", "\n");

            // Quita saltos de línea al final (porque luego añadimos uno exacto)
            s = s.TrimEnd('\n', ' ', '\t');

            // Asegura newline final para que el siguiente bloque no “pegue” el ER con TY
            return s + "\n";
        }

        // Opcional por si quieres validar fácil (no lo uso en Exportar para mantenerlo simple)
        public bool PareceBloqueRisValido(string bloque)
        {
            if (string.IsNullOrWhiteSpace(bloque))
                return false;

            var s = bloque.Replace("\r\n", "\n").Replace("\r", "\n");

            // Comprobación muy simple: contiene una línea que empieza por TY y otra por ER
            return s.Contains("\nTY") || s.StartsWith("TY")
                ? (s.Contains("\nER") || s.StartsWith("ER") || s.Contains("\nER"))
                : false;
        }
    }
}
