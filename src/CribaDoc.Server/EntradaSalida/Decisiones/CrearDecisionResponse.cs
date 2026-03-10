using CribaDoc.Server.EntradaSalida.Busquedas;

namespace CribaDoc.Server.EntradaSalida.Decisiones
{
    public class CrearDecisionResponse
    {
        public int MarcadorAntes { get; set; }

        public int MarcadorDespues { get; set; }

        public EstadisticasBusquedaResponse? Stats { get; set; }
    }
}
