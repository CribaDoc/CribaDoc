using System;
using System.Collections.Generic;
using CribaDoc.Server.Persistencia.Entidades;
using Npgsql;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public class DecisionRepositorio : IDecisionRepositorio
    {
        private readonly FabricaConexion _fabricaConexion;

        public DecisionRepositorio(FabricaConexion fabricaConexion)
        {
            _fabricaConexion = fabricaConexion;
        }

        public long Insertar(long busquedaId, long paperId, int tipo, string? nota)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                INSERT INTO decision (busqueda_id, paper_id, tipo, nota)
                VALUES (@busquedaId, @paperId, @tipo, @nota)
                RETURNING id;
                """,
                conn);

            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("paperId", paperId);
            cmd.Parameters.AddWithValue("tipo", tipo);
            cmd.Parameters.AddWithValue("nota", (object?)nota ?? DBNull.Value);

            return (long)cmd.ExecuteScalar()!;
        }

        public int ContarPorBusquedaYTipo(long busquedaId, int tipo)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT COUNT(*)
                FROM decision
                WHERE busqueda_id = @busquedaId
                  AND tipo = @tipo;
                """,
                conn);

            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("tipo", tipo);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<DecisionEntity> ListarPorBusqueda(long busquedaId)
        {
            var decisiones = new List<DecisionEntity>();

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT id, busqueda_id, paper_id, tipo, nota
                FROM decision
                WHERE busqueda_id = @busquedaId
                ORDER BY id;
                """,
                conn);

            cmd.Parameters.AddWithValue("busquedaId", busquedaId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                decisiones.Add(LeerDecision(reader));
            }

            return decisiones;
        }

        public List<DecisionEntity> ListarPorProyecto(long proyectoId)
        {
            var decisiones = new List<DecisionEntity>();

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT d.id, d.busqueda_id, d.paper_id, d.tipo, d.nota
                FROM decision d
                INNER JOIN busqueda b ON b.id = d.busqueda_id
                WHERE b.proyecto_id = @proyectoId
                ORDER BY d.id;
                """,
                conn);

            cmd.Parameters.AddWithValue("proyectoId", proyectoId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                decisiones.Add(LeerDecision(reader));
            }

            return decisiones;
        }

        private static DecisionEntity LeerDecision(NpgsqlDataReader reader)
        {
            return new DecisionEntity
            {
                Id = reader.GetInt64(0),
                BusquedaId = reader.GetInt64(1),
                PaperId = reader.GetInt64(2),
                Tipo = reader.GetInt32(3),
                Nota = reader.IsDBNull(4) ? null : reader.GetString(4)
            };
        }
    }
}