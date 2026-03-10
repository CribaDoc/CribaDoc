using Microsoft.AspNetCore.Mvc;
using CribaDoc.Server.EntradaSalida.Auth;
using CribaDoc.Server.Negocio;

namespace CribaDoc.Server.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public ActionResult<RegistrarUsuarioResponse> Register([FromBody] RegistrarUsuarioRequest request)
        {
            var response = _authService.Registrar(request);
            return Ok(response);
        }

        [HttpPost("login")]
        public ActionResult<LoginUsuarioResponse> Login([FromBody] LoginUsuarioRequest request)
        {
            var response = _authService.Login(request);
            return Ok(response);
        }
    }
}
