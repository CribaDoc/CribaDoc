using System.Collections.Generic;
using CribaDoc.Server.Persistencia.Entidades;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public interface IPaperRepositorio
    {
        void InsertarVarios(List<PaperEntity> papers);

        PaperEntity? ObtenerActual(long busquedaId, int ordenOriginal);

        int ContarPorBusqueda(long busquedaId);

        List<PaperEntity> ListarPorBusqueda(long busquedaId);

        List<string> ObtenerBloquesRisIncluidosPorBusqueda(long busquedaId);

        List<string> ObtenerBloquesRisIncluidosPorProyecto(long proyectoId);
    }
}
