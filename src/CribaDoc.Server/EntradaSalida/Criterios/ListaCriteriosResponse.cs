using System.Collections.Generic;

namespace CribaDoc.Server.EntradaSalida.Criterios
{
    public class ListaCriteriosResponse
    {
        public List<CriterioItem> Inclusion { get; set; } = new();

        public List<CriterioItem> Exclusion { get; set; } = new();
    }
}
