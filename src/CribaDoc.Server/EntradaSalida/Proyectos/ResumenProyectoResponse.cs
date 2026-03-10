using System.Collections.Generic;

namespace CribaDoc.Server.EntradaSalida.Proyectos
{
    public class ResumenProyectoResponse
    {
        public string? Nombre { get; set; }

        public List<BusquedaResumenItem> Busquedas { get; set; } = new List<BusquedaResumenItem>();
    }

    public class BusquedaResumenItem
    {
        public long BusquedaId { get; set; }

        public int Orden { get; set; }

        public int Total { get; set; }

        public int Result { get; set; }

        public int Deleted { get; set; }

        public int Marcador { get; set; }
    }
}
