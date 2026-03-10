using System.Collections.Generic;
using CribaDoc.Server.Persistencia.Entidades;
using Npgsql;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public class CriterioRepositorio : ICriterioRepositorio
    {
        private readonly FabricaConexion _fabricaConexion;

        public CriterioRepositorio(FabricaConexion fabricaConexion)
        {
            _fabricaConexion = fabricaConexion;
        }

        public void Insertar(long busquedaId, int tipo, string texto)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                INSERT INTO criterio (busqueda_id, tipo, texto)
                VALUES (@busquedaId, @tipo, @texto);
                """,
                conn);

            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("tipo", tipo);
            cmd.Parameters.AddWithValue("texto", texto);

            cmd.ExecuteNonQuery();
        }

        public List<CriterioEntity> ListarPorBusqueda(long busquedaId)
        {
            var criterios = new List<CriterioEntity>();

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT id, busqueda_id, tipo, texto
                FROM criterio
                WHERE busqueda_id = @busquedaId
                ORDER BY id;
                """,
                conn);

            cmd.Parameters.AddWithValue("busquedaId", busquedaId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                criterios.Add(new CriterioEntity
                {
                    Id = reader.GetInt64(0),
                    BusquedaId = reader.GetInt64(1),
                    Tipo = reader.GetInt32(2),
                    Texto = reader.GetString(3)
                });
            }

            return criterios;
        }
    }
}