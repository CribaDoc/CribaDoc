using System.Collections.Generic;

namespace CribaDoc.Server.EntradaSalida.Decisiones
{
    public class CrearDecisionRequest
    {
        public int Tipo { get; set; }   // 1 = Result, 2 = Deleted

        public string? Nota { get; set; }

        public List<long> CriteriosAplicados { get; set; } = new();
    }
}