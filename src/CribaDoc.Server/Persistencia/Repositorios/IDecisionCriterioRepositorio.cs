using System.Collections.Generic;
using CribaDoc.Server.Persistencia.Entidades;

namespace CribaDoc.Server.Persistencia.Repositorios
{
    public interface IDecisionCriterioRepositorio
    {
        void Insertar(long decisionId, long criterioId);

        void InsertarVarios(long decisionId, List<long> criteriosId);

        List<DecisionCriterioEntity> ListarPorDecision(long decisionId);
    }
}
