using System.Collections.Generic;
using CribaDoc.Server.Persistencia.Entidades;
using Npgsql;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public class ProyectoRepositorio : IProyectoRepositorio
    {
        private readonly FabricaConexion _fabricaConexion;

        public ProyectoRepositorio(FabricaConexion fabricaConexion)
        {
            _fabricaConexion = fabricaConexion;
        }

        public ProyectoEntity? ObtenerPorId(long id)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"
                SELECT id, usuario_id, nombre, password_hash
                FROM proyecto
                WHERE id = @id
                LIMIT 1;
                ",
                conn);

            cmd.Parameters.AddWithValue("id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return Leer(reader);
        }

        public ProyectoEntity? ObtenerPorUsuarioYNombre(long usuarioId, string nombre)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"
                SELECT id, usuario_id, nombre, password_hash
                FROM proyecto
                WHERE usuario_id = @usuarioId
                  AND LOWER(nombre) = LOWER(@nombre)
                LIMIT 1;
                ",
                conn);

            cmd.Parameters.AddWithValue("usuarioId", usuarioId);
            cmd.Parameters.AddWithValue("nombre", nombre);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return Leer(reader);
        }

        public List<ProyectoEntity> ListarPorUsuario(long usuarioId)
        {
            var proyectos = new List<ProyectoEntity>();

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"
                SELECT id, usuario_id, nombre, password_hash
                FROM proyecto
                WHERE usuario_id = @usuarioId
                ORDER BY id;
                ",
                conn);

            cmd.Parameters.AddWithValue("usuarioId", usuarioId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                proyectos.Add(Leer(reader));
            }

            return proyectos;
        }

        public long Insertar(long usuarioId, string nombre, string passwordHash)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"
                INSERT INTO proyecto (usuario_id, nombre, password_hash)
                VALUES (@usuarioId, @nombre, @passwordHash)
                RETURNING id;
                ",
                conn);

            cmd.Parameters.AddWithValue("usuarioId", usuarioId);
            cmd.Parameters.AddWithValue("nombre", nombre);
            cmd.Parameters.AddWithValue("passwordHash", passwordHash);

            return (long)cmd.ExecuteScalar()!;
        }

        private static ProyectoEntity Leer(NpgsqlDataReader reader)
        {
            return new ProyectoEntity
            {
                Id = reader.GetInt64(0),
                UsuarioId = reader.GetInt64(1),
                Nombre = reader.GetString(2),
                PasswordHash = reader.GetString(3)
            };
        }
    }
}