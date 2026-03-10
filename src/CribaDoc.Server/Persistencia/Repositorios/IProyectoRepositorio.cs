using CribaDoc.Server.Persistencia.Entidades;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public interface IProyectoRepositorio
    {
        ProyectoEntity? ObtenerPorId(long id);

        ProyectoEntity? ObtenerPorUsuarioYNombre(long usuarioId, string nombre);

        List<ProyectoEntity> ListarPorUsuario(long usuarioId);

        long Insertar(long usuarioId, string nombre, string passwordHash);
    }
}
