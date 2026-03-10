using System.Collections.Generic;
using CribaDoc.Server.Persistencia.Entidades;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public interface ICriterioRepositorio
    {
        void Insertar(long busquedaId, int tipo, string texto);

        List<CriterioEntity> ListarPorBusqueda(long busquedaId);
    }
}