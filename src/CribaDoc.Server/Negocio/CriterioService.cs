using System;
using CribaDoc.Server.EntradaSalida.Criterios;
using CribaDoc.Server.Persistencia.Repositorios;

namespace CribaDoc.Server.Negocio
{
    public class CriterioService
    {
        private readonly ICriterioRepositorio _criterioRepositorio;

        public CriterioService(ICriterioRepositorio criterioRepositorio)
        {
            _criterioRepositorio = criterioRepositorio;
        }

        public void Crear(long busquedaId, CrearCriterioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Texto))
                throw new ArgumentException("El texto del criterio es obligatorio");

            if (request.Tipo != 1 && request.Tipo != 2)
                throw new ArgumentException("Tipo de criterio inválido");

            _criterioRepositorio.Insertar(busquedaId, request.Tipo, request.Texto);
        }

        public ListaCriteriosResponse ListarPorBusqueda(long busquedaId)
        {
            var criterios = _criterioRepositorio.ListarPorBusqueda(busquedaId);

            var response = new ListaCriteriosResponse();

            foreach (var c in criterios)
            {
                var item = new CriterioItem
                {
                    Id = c.Id,
                    Tipo = c.Tipo,
                    Texto = c.Texto
                };

                if (c.Tipo == 1)
                    response.Inclusion.Add(item);
                else
                    response.Exclusion.Add(item);
            }

            return response;
        }
    }
}