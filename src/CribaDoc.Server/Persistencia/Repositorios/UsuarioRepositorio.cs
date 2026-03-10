using CribaDoc.Server.Persistencia.Entidades;
using Npgsql;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly FabricaConexion _fabricaConexion;

        public UsuarioRepositorio(FabricaConexion fabricaConexion)
        {
            _fabricaConexion = fabricaConexion;
        }

        public UsuarioEntity? ObtenerPorId(long id)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"
                SELECT id, nombre_usuario, password_hash
                FROM usuario
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

        public UsuarioEntity? ObtenerPorNombreUsuario(string nombreUsuario)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"
                SELECT id, nombre_usuario, password_hash
                FROM usuario
                WHERE LOWER(nombre_usuario) = LOWER(@nombreUsuario)
                LIMIT 1;
                ",
                conn);

            cmd.Parameters.AddWithValue("nombreUsuario", nombreUsuario);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return Leer(reader);
        }

        public long Insertar(string nombreUsuario, string passwordHash)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"
                INSERT INTO usuario (nombre_usuario, password_hash)
                VALUES (@nombreUsuario, @passwordHash)
                RETURNING id;
                ",
                conn);

            cmd.Parameters.AddWithValue("nombreUsuario", nombreUsuario);
            cmd.Parameters.AddWithValue("passwordHash", passwordHash);

            return (long)cmd.ExecuteScalar()!;
        }

        private static UsuarioEntity Leer(NpgsqlDataReader reader)
        {
            return new UsuarioEntity
            {
                Id = reader.GetInt64(0),
                NombreUsuario = reader.GetString(1),
                PasswordHash = reader.GetString(2)
            };
        }
    }
}