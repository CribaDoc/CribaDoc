using Npgsql;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public class AccesoProyectoRepositorio : IAccesoProyectoRepositorio
    {
        private readonly FabricaConexion _fabricaConexion;

        public AccesoProyectoRepositorio(FabricaConexion fabricaConexion)
        {
            _fabricaConexion = fabricaConexion;
        }

        public bool ProyectoPerteneceAUsuario(long proyectoId, long usuarioId)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT 1
                FROM proyecto
                WHERE id = @proyectoId
                  AND usuario_id = @usuarioId
                LIMIT 1;
                """,
                conn);

            cmd.Parameters.AddWithValue("proyectoId", proyectoId);
            cmd.Parameters.AddWithValue("usuarioId", usuarioId);

            return cmd.ExecuteScalar() != null;
        }

        public bool BusquedaPerteneceAProyecto(long busquedaId, long proyectoId)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT 1
                FROM busqueda
                WHERE id = @busquedaId
                  AND proyecto_id = @proyectoId
                LIMIT 1;
                """,
                conn);

            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("proyectoId", proyectoId);

            return cmd.ExecuteScalar() != null;
        }
    }
}