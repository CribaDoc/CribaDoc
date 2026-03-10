using CribaDoc.Server.Persistencia.Entidades;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public interface IUsuarioRepositorio
    {
        UsuarioEntity? ObtenerPorId(long id);

        UsuarioEntity? ObtenerPorNombreUsuario(string nombreUsuario);

        long Insertar(string nombreUsuario, string passwordHash);
    }
}
