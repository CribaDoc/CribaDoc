using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CribaDoc.Server.Auth;
using CribaDoc.Server.EntradaSalida.Criterios;
using CribaDoc.Server.Negocio;
using CribaDoc.Server.Persistencia.Repositorios;

namespace CribaDoc.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("busquedas/{id:long}/criterios")]
    public class CriteriosController : ControllerBase
    {
        private readonly CriterioService _criterioService;
        private readonly IAccesoProyectoRepositorio _accesoProyectoRepositorio;

        public CriteriosController(
            CriterioService criterioService,
            IAccesoProyectoRepositorio accesoProyectoRepositorio)
        {
            _criterioService = criterioService;
            _accesoProyectoRepositorio = accesoProyectoRepositorio;
        }

        [HttpGet]
        public ActionResult<ListaCriteriosResponse> Get(long id)
        {
            if (User.GetScope() != "project")
                return Forbid();

            var projectId = User.GetProjectId();
            if (projectId == null || !_accesoProyectoRepositorio.BusquedaPerteneceAProyecto(id, projectId.Value))
                return Forbid();

            var result = _criterioService.ListarPorBusqueda(id);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post(long id, [FromBody] CrearCriterioRequest request)
        {
            if (User.GetScope() != "project")
                return Forbid();

            var projectId = User.GetProjectId();
            if (projectId == null || !_accesoProyectoRepositorio.BusquedaPerteneceAProyecto(id, projectId.Value))
                return Forbid();

            if (request == null)
                return BadRequest();

            _criterioService.Crear(id, request);
            return Ok();
        }
    }
}
