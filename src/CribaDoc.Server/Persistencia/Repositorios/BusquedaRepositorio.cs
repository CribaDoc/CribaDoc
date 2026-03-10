using System.Collections.Generic;
using CribaDoc.Server.Persistencia.Entidades;
using Npgsql;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public class BusquedaRepositorio : IBusquedaRepositorio
    {
        private readonly FabricaConexion _fabricaConexion;

        public BusquedaRepositorio(FabricaConexion fabricaConexion)
        {
            _fabricaConexion = fabricaConexion;
        }

        public long Insertar(long proyectoId, int orden, string risTextoOriginal)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                INSERT INTO busqueda (proyecto_id, orden, ris_texto_original, marcador)
                VALUES (@proyectoId, @orden, @risTextoOriginal, 0)
                RETURNING id;
                """,
                conn);

            cmd.Parameters.AddWithValue("proyectoId", proyectoId);
            cmd.Parameters.AddWithValue("orden", orden);
            cmd.Parameters.AddWithValue("risTextoOriginal", risTextoOriginal);

            return (long)cmd.ExecuteScalar()!;
        }

        public BusquedaEntity? ObtenerPorId(long id)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT id, proyecto_id, orden, ris_texto_original, marcador
                FROM busqueda
                WHERE id = @id;
                """,
                conn);

            cmd.Parameters.AddWithValue("id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new BusquedaEntity
            {
                Id = reader.GetInt64(0),
                ProyectoId = reader.GetInt64(1),
                Orden = reader.GetInt32(2),
                RisTextoOriginal = reader.GetString(3),
                Marcador = reader.GetInt32(4)
            };
        }

        public List<BusquedaEntity> ListarPorProyecto(long proyectoId)
        {
            var busquedas = new List<BusquedaEntity>();

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT id, proyecto_id, orden, ris_texto_original, marcador
                FROM busqueda
                WHERE proyecto_id = @proyectoId
                ORDER BY orden;
                """,
                conn);

            cmd.Parameters.AddWithValue("proyectoId", proyectoId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                busquedas.Add(new BusquedaEntity
                {
                    Id = reader.GetInt64(0),
                    ProyectoId = reader.GetInt64(1),
                    Orden = reader.GetInt32(2),
                    RisTextoOriginal = reader.GetString(3),
                    Marcador = reader.GetInt32(4)
                });
            }

            return busquedas;
        }

        public int ObtenerSiguienteOrden(long proyectoId)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT COALESCE(MAX(orden), 0) + 1
                FROM busqueda
                WHERE proyecto_id = @proyectoId;
                """,
                conn);

            cmd.Parameters.AddWithValue("proyectoId", proyectoId);

            return (int)cmd.ExecuteScalar()!;
        }

        public void ActualizarMarcador(long busquedaId, int nuevoMarcador)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                UPDATE busqueda
                SET marcador = @nuevoMarcador
                WHERE id = @busquedaId;
                """,
                conn);

            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("nuevoMarcador", nuevoMarcador);

            cmd.ExecuteNonQuery();
        }
    }
}
