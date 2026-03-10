using System;
using System.Collections.Generic;
using System.Text;

namespace CribaDoc.Core.Ris
{
    /// Convierte un texto RIS completo en un objeto Ris (lista de papers TY..ER)
    public sealed class ConversorRis : IConversorRis
    {
        public Ris Parse(string risText)
        {
            if (string.IsNullOrWhiteSpace(risText))
                return new Ris(new List<RisPaper>());

            var papers = new List<RisPaper>();

            // Normalizamos saltos de línea para facilitar el parseo
            var text = risText.Replace("\r\n", "\n").Replace("\r", "\n");
            var lines = text.Split('\n');

            var inBlock = false;
            var currentBlock = new StringBuilder();
            var currentTags = new List<RisTag>();
            RisTag? lastTag = null;

            foreach (var raw in lines)
            {
                var line = raw ?? string.Empty;

                // Detectar inicio de bloque
                if (!inBlock)
                {
                    if (EsComienzo(line))
                    {
                        inBlock = true;
                        currentBlock.Clear();
                        currentTags.Clear();
                        lastTag = null;

                        AppendLine(currentBlock, line);
                        lastTag = AddTag(line, currentTags);
                    }

                    continue;
                }

                // Estamos dentro de un bloque
                AppendLine(currentBlock, line);

                if (EsLineaTag(line))
                {
                    lastTag = AddTag(line, currentTags);

                    // Detectar fin de bloque
                    if (EsFinal(line))
                    {
                        var blockText = currentBlock.ToString();

                        // IMPORTANTE: copiar la lista antes de limpiar
                        papers.Add(new RisPaper(blockText, new List<RisTag>(currentTags)));

                        inBlock = false;
                        currentBlock.Clear();
                        currentTags.Clear();
                        lastTag = null;
                    }
                }
                else
                {
                    // Línea de continuación: se concatena al último tag
                    if (lastTag != null && !string.IsNullOrWhiteSpace(line))
                    {
                        var nuevoValor = string.IsNullOrWhiteSpace(lastTag.Value)
                            ? line.Trim()
                            : $"{lastTag.Value} {line.Trim()}";

                        currentTags[currentTags.Count - 1] = new RisTag(lastTag.Tag, nuevoValor);
                        lastTag = currentTags[currentTags.Count - 1];
                    }
                }
            }

            // Cierre defensivo por si un bloque no termina con ER
            if (inBlock && currentTags.Count > 0)
            {
                var blockText = currentBlock.ToString();
                papers.Add(new RisPaper(blockText, new List<RisTag>(currentTags)));
            }

            return new Ris(papers);
        }

        private static bool EsComienzo(string line)
        {
            var t = line.TrimStart();
            return t.StartsWith("TY", StringComparison.OrdinalIgnoreCase) && t.Contains("-");
        }

        private static bool EsFinal(string line)
        {
            var t = line.TrimStart();
            return t.StartsWith("ER", StringComparison.OrdinalIgnoreCase) && t.Contains("-");
        }

        private static bool EsLineaTag(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            int idx = line.IndexOf('-');
            if (idx <= 0)
                return false;

            var tag = line.Substring(0, idx).Trim();
            return tag.Length >= 2;
        }

        private static RisTag? AddTag(string line, List<RisTag> tags)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            int idx = line.IndexOf('-');
            if (idx <= 0)
                return null;

            var tag = line.Substring(0, idx).Trim().ToUpperInvariant();
            var value = line.Substring(idx + 1).Trim();

            if (tag.Length == 0)
                return null;

            try
            {
                var risTag = new RisTag(tag, value);
                tags.Add(risTag);
                return risTag;
            }
            catch
            {
                // Ignoramos líneas raras para mantener tolerancia
                return null;
            }
        }

        private static void AppendLine(StringBuilder sb, string line)
        {
            sb.Append(line);
            sb.Append('\n');
        }
    }
}