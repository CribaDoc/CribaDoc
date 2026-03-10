using System.Collections.Generic;
using CribaDoc.Server.Persistencia.Entidades;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public interface IDecisionRepositorio
    {
        long Insertar(long busquedaId, long paperId, int tipo, string? nota);

        int ContarPorBusquedaYTipo(long busquedaId, int tipo);

        List<DecisionEntity> ListarPorBusqueda(long busquedaId);

        List<DecisionEntity> ListarPorProyecto(long proyectoId);
    }
}