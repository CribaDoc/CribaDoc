using System;

namespace CribaDoc.Core.Ris
{
    /// Representa una línea RIS interpretada: "AU  - XXXXXX"
    public sealed class RisTag
    {
        public string Tag { get; }
        public string Value { get; }

        public RisTag(string tag, string value)
        {
            Tag = (tag ?? string.Empty).Trim();
            Value = (value ?? string.Empty).Trim();
            if (Tag.Length == 0)
            {
                throw new ArgumentException("RIS tag no puede estar vacío.", nameof(tag));
            }
        }

        public override string ToString()
        {
            // Formato RIS estándar (aprox.): "TAG  - VALUE"
            return $"{Tag}  - {Value}";
        }
    }
}
