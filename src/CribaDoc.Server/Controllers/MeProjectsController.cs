using CribaDoc.Server.Auth;
using CribaDoc.Server.EntradaSalida.Proyectos;
using CribaDoc.Server.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CribaDoc.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("me/projects")]
    public class MeProjectsController : ControllerBase
    {
        private readonly ProyectoService _proyectoService;

        public MeProjectsController(ProyectoService proyectoService)
        {
            _proyectoService = proyectoService;
        }

        [HttpGet]
        public ActionResult<ListaMisProyectosResponse> Get()
        {
            if (User.GetScope() != "user")
                return Forbid();

            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var response = _proyectoService.ListarPorUsuario(userId.Value);
            return Ok(response);
        }

        [HttpPost]
        public ActionResult<CrearProyectoResponse> Post([FromBody] CrearProyectoRequest request)
        {
            if (User.GetScope() != "user")
                return Forbid();

            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var response = _proyectoService.CrearParaUsuario(userId.Value, request);
            return Ok(response);
        }

        [HttpPost("{id:long}/open")]
        public ActionResult<AbrirProyectoResponse> Open(long id)
        {
            if (User.GetScope() != "user")
                return Forbid();

            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var response = _proyectoService.AbrirPropio(userId.Value, id);
            return Ok(response);
        }
    }
}