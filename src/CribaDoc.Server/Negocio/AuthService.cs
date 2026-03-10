using CribaDoc.Server.Auth;
using CribaDoc.Server.EntradaSalida.Auth;
using CribaDoc.Server.Persistencia.Repositorios;

namespace CribaDoc.Server.Negocio
{
    public class AuthService
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly PasswordHasher _passwordHasher;
        private readonly AppTokenService _tokenService;

        public AuthService(
            IUsuarioRepositorio usuarioRepositorio,
            PasswordHasher passwordHasher,
            AppTokenService tokenService)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public RegistrarUsuarioResponse Registrar(RegistrarUsuarioRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.NombreUsuario))
                throw new ArgumentException("El nombre de usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("La contraseña es obligatoria.");

            var existente = _usuarioRepositorio.ObtenerPorNombreUsuario(request.NombreUsuario);
            if (existente != null)
                throw new InvalidOperationException("Ese nombre de usuario ya existe.");

            var hash = _passwordHasher.Hash(request.Password);
            var id = _usuarioRepositorio.Insertar(request.NombreUsuario, hash);
            var token = _tokenService.CreateUserToken(id, request.NombreUsuario);

            return new RegistrarUsuarioResponse
            {
                Id = id,
                NombreUsuario = request.NombreUsuario,
                Token = token
            };
        }

        public LoginUsuarioResponse Login(LoginUsuarioRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.NombreUsuario))
                throw new ArgumentException("El nombre de usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("La contraseña es obligatoria.");

            var usuario = _usuarioRepositorio.ObtenerPorNombreUsuario(request.NombreUsuario);
            if (usuario == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            var ok = _passwordHasher.Verify(request.Password, usuario.PasswordHash);
            if (!ok)
                throw new InvalidOperationException("Contraseña incorrecta.");

            var token = _tokenService.CreateUserToken(usuario.Id, usuario.NombreUsuario);

            return new LoginUsuarioResponse
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                Token = token
            };
        }
    }
}
