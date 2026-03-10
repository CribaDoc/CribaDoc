namespace CribaDoc.Server.Persistencia.Repositorios
{
    public interface IAccesoProyectoRepositorio
    {
        bool ProyectoPerteneceAUsuario(long proyectoId, long usuarioId);

        bool BusquedaPerteneceAProyecto(long busquedaId, long proyectoId);
    }
}