using System.Collections.Generic;

namespace CribaDoc.Core.Ris
{
    /// Representa un RIS completo ya parseado
    public sealed class Ris
    {
        public List<RisPaper> Papers { get; }

        public Ris(List<RisPaper> papers)
        {
            Papers = papers ?? new List<RisPaper>();
        }
        public int Count()
        {
            return Papers.Count;
        }
        public RisPaper? Get(int index)
        {
            if (index < 0 || index >= Papers.Count)
                return null;

            return Papers[index];
        }
        public bool IsEmpty()
        {
            return Papers.Count == 0;
        }
    }
}
