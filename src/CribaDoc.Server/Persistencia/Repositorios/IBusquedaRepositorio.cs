using System.Collections.Generic;
using CribaDoc.Server.Persistencia.Entidades;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public interface IBusquedaRepositorio
    {
        long Insertar(long proyectoId, int orden, string risTextoOriginal);

        BusquedaEntity? ObtenerPorId(long id);

        List<BusquedaEntity> ListarPorProyecto(long proyectoId);

        int ObtenerSiguienteOrden(long proyectoId);

        void ActualizarMarcador(long busquedaId, int nuevoMarcador);
    }
}
