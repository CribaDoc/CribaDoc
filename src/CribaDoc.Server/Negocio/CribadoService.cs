using System;
using System.Collections.Generic;
using System.Linq;
using CribaDoc.Server.EntradaSalida.Busquedas;
using CribaDoc.Server.EntradaSalida.Decisiones;
using CribaDoc.Server.Persistencia;
using Npgsql;

namespace CribaDoc.Server.Negocio
{
    public class CribadoService
    {
        private readonly FabricaConexion _fabricaConexion;

        public CribadoService(FabricaConexion fabricaConexion)
        {
            _fabricaConexion = fabricaConexion;
        }

        public CrearDecisionResponse CrearDecision(long busquedaId, CrearDecisionRequest request)
        {
            if (busquedaId <= 0)
                throw new ArgumentException("La búsqueda es obligatoria.");

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Tipo != 1 && request.Tipo != 2)
                throw new ArgumentException("El tipo de decisión debe ser 1 o 2.");

            using var conn = _fabricaConexion.Crear();
            conn.Open();

            using var tx = conn.BeginTransaction();

            var marcadorAntes = ObtenerMarcadorBloqueando(conn, tx, busquedaId);
            var total = ContarPapers(conn, tx, busquedaId);

            if (total == 0 || marcadorAntes >= total)
                throw new InvalidOperationException("No quedan papers por cribar en esta búsqueda.");

            var paperId = ObtenerPaperIdActual(conn, tx, busquedaId, marcadorAntes);
            if (paperId == null)
                throw new InvalidOperationException("No se ha podido localizar el paper actual.");

            var decisionId = InsertarDecision(
                conn,
                tx,
                busquedaId,
                paperId.Value,
                request.Tipo,
                request.Nota
            );

            InsertarDecisionCriterios(
                conn,
                tx,
                busquedaId,
                decisionId,
                request.CriteriosAplicados ?? new List<long>()
            );

            ActualizarMarcador(conn, tx, busquedaId, marcadorAntes + 1);

            var stats = ObtenerEstadisticasInternas(conn, tx, busquedaId);

            tx.Commit();

            return new CrearDecisionResponse
            {
                MarcadorAntes = marcadorAntes,
                MarcadorDespues = marcadorAntes + 1,
                Stats = stats
            };
        }

        private static int ObtenerMarcadorBloqueando(NpgsqlConnection conn, NpgsqlTransaction tx, long busquedaId)
        {
            const string sql = @"
                SELECT marcador
                FROM busqueda
                WHERE id = @busquedaId
                FOR UPDATE;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);

            var result = cmd.ExecuteScalar();
            if (result == null)
                throw new InvalidOperationException("La búsqueda no existe.");

            return Convert.ToInt32(result);
        }

        private static int ContarPapers(NpgsqlConnection conn, NpgsqlTransaction tx, long busquedaId)
        {
            const string sql = @"SELECT COUNT(*) FROM paper WHERE busqueda_id = @busquedaId;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);

            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        private static long? ObtenerPaperIdActual(
            NpgsqlConnection conn,
            NpgsqlTransaction tx,
            long busquedaId,
            int offset)
        {
            const string sql = @"
                SELECT id
                FROM paper
                WHERE busqueda_id = @busquedaId
                ORDER BY orden_original
                OFFSET @offset
                LIMIT 1;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("offset", offset);

            var result = cmd.ExecuteScalar();
            if (result == null)
                return null;

            return Convert.ToInt64(result);
        }

        private static long InsertarDecision(
            NpgsqlConnection conn,
            NpgsqlTransaction tx,
            long busquedaId,
            long paperId,
            int tipo,
            string? nota)
        {
            const string sql = @"
                INSERT INTO decision
                (
                    busqueda_id,
                    paper_id,
                    tipo,
                    nota
                )
                VALUES
                (
                    @busquedaId,
                    @paperId,
                    @tipo,
                    @nota
                )
                RETURNING id;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("paperId", paperId);
            cmd.Parameters.AddWithValue("tipo", tipo);
            cmd.Parameters.AddWithValue("nota", string.IsNullOrWhiteSpace(nota) ? DBNull.Value : nota);

            return Convert.ToInt64(cmd.ExecuteScalar()!);
        }

        private static void InsertarDecisionCriterios(
            NpgsqlConnection conn,
            NpgsqlTransaction tx,
            long busquedaId,
            long decisionId,
            List<long> criterioIds)
        {
            foreach (var criterioId in criterioIds.Distinct())
            {
                const string sql = @"
                    INSERT INTO decision_criterio (decision_id, criterio_id)
                    SELECT @decisionId, @criterioId
                    WHERE EXISTS
                    (
                        SELECT 1
                        FROM criterio
                        WHERE id = @criterioId
                          AND busqueda_id = @busquedaId
                    );";

                using var cmd = new NpgsqlCommand(sql, conn, tx);
                cmd.Parameters.AddWithValue("decisionId", decisionId);
                cmd.Parameters.AddWithValue("criterioId", criterioId);
                cmd.Parameters.AddWithValue("busquedaId", busquedaId);

                cmd.ExecuteNonQuery();
            }
        }

        private static void ActualizarMarcador(
            NpgsqlConnection conn,
            NpgsqlTransaction tx,
            long busquedaId,
            int nuevoMarcador)
        {
            const string sql = @"
                UPDATE busqueda
                SET marcador = @marcador
                WHERE id = @busquedaId;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("marcador", nuevoMarcador);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);

            cmd.ExecuteNonQuery();
        }

        private static EstadisticasBusquedaResponse ObtenerEstadisticasInternas(
            NpgsqlConnection conn,
            NpgsqlTransaction tx,
            long busquedaId)
        {
            var total = ContarPapers(conn, tx, busquedaId);
            var result = ContarDecisiones(conn, tx, busquedaId, 1);
            var deleted = ContarDecisiones(conn, tx, busquedaId, 2);
            var marcador = ObtenerMarcador(conn, tx, busquedaId);

            return new EstadisticasBusquedaResponse
            {
                Total = total,
                Result = result,
                Deleted = deleted,
                Marcador = marcador
            };
        }

        private static int ContarDecisiones(
            NpgsqlConnection conn,
            NpgsqlTransaction tx,
            long busquedaId,
            int tipo)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM decision
                WHERE busqueda_id = @busquedaId
                  AND tipo = @tipo;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);
            cmd.Parameters.AddWithValue("tipo", tipo);

            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        private static int ObtenerMarcador(NpgsqlConnection conn, NpgsqlTransaction tx, long busquedaId)
        {
            const string sql = @"SELECT marcador FROM busqueda WHERE id = @busquedaId;";

            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("busquedaId", busquedaId);

            var result = cmd.ExecuteScalar();
            if (result == null)
                throw new InvalidOperationException("La búsqueda no existe.");

            return Convert.ToInt32(result);
        }
    }
}