using System.Collections.Generic;
using CribaDoc.Server.Persistencia.Entidades;
using Npgsql;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public class DecisionCriterioRepositorio : IDecisionCriterioRepositorio
    {
        private readonly FabricaConexion _fabricaConexion;

        public DecisionCriterioRepositorio(FabricaConexion fabricaConexion)
        {
            _fabricaConexion = fabricaConexion;
        }

        public void Insertar(long decisionId, long criterioId)
        {
            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                INSERT INTO decision_criterio (decision_id, criterio_id)
                VALUES (@decisionId, @criterioId);
                """,
                conn);

            cmd.Parameters.AddWithValue("decisionId", decisionId);
            cmd.Parameters.AddWithValue("criterioId", criterioId);

            cmd.ExecuteNonQuery();
        }

        public void InsertarVarios(long decisionId, List<long> criteriosId)
        {
            if (criteriosId == null || criteriosId.Count == 0)
                return;

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            foreach (var criterioId in criteriosId)
            {
                using var cmd = new NpgsqlCommand(
                    """
                    INSERT INTO decision_criterio (decision_id, criterio_id)
                    VALUES (@decisionId, @criterioId);
                    """,
                    conn);

                cmd.Parameters.AddWithValue("decisionId", decisionId);
                cmd.Parameters.AddWithValue("criterioId", criterioId);

                cmd.ExecuteNonQuery();
            }
        }

        public List<DecisionCriterioEntity> ListarPorDecision(long decisionId)
        {
            var relaciones = new List<DecisionCriterioEntity>();

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                """
                SELECT decision_id, criterio_id
                FROM decision_criterio
                WHERE decision_id = @decisionId
                ORDER BY criterio_id;
                """,
                conn);

            cmd.Parameters.AddWithValue("decisionId", decisionId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                relaciones.Add(new DecisionCriterioEntity
                {
                    DecisionId = reader.GetInt64(0),
                    CriterioId = reader.GetInt64(1)
                });
            }

            return relaciones;
        }
    }
}