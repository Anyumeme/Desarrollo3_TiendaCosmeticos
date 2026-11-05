using TiendaCosmeticos.Controllers;
using TiendaCosmeticos.Data;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;
using TiendaCosmeticos.Services;

namespace TiendaCosmeticos.Util
{
    public static class Startup
    {
        private static AdministradorRepository? _adminRepository;
        private static EmpleadoRepository? _empleadoRepository;
        private static ClienteRepository? _clienteRepository;
        private static ProductoRepository? _productoRepository;
        private static ContratoRepository? _contratoRepository;
        private static VentaRepository? _ventaRepository;
        private static IConsoleWriter? _consoleWriter;
        private static bool _seedsCargadas = false;

        public static IConsoleWriter InitConsoleWriter()
        {
            if (_consoleWriter == null)
            {
                _consoleWriter = new ConsoleWriter();
            }
            return _consoleWriter;
        }

        private static void AsegurarRepositorios()
        {
            if (_adminRepository == null) _adminRepository = new AdministradorRepository();
            if (_empleadoRepository == null) _empleadoRepository = new EmpleadoRepository();
            if (_clienteRepository == null) _clienteRepository = new ClienteRepository();
            if (_productoRepository == null) _productoRepository = new ProductoRepository();
            if (_contratoRepository == null) _contratoRepository = new ContratoRepository();
            if (_ventaRepository == null) _ventaRepository = new VentaRepository();

            if (!_seedsCargadas)
            {
                _seedsCargadas = true;
                Seeds.Inicializar(
                    _adminRepository,
                    _empleadoRepository,
                    _clienteRepository,
                    _productoRepository,
                    _contratoRepository
                );
            }
        }

        public static AdministradorRepository GetAdministradorRepository()
        {
            AsegurarRepositorios();
            return _adminRepository!;
        }

        public static EmpleadoRepository GetEmpleadoRepository()
        {
            AsegurarRepositorios();
            return _empleadoRepository!;
        }

        public static ClienteRepository GetClienteRepository()
        {
            AsegurarRepositorios();
            return _clienteRepository!;
        }

        public static ProductoRepository GetProductoRepository()
        {
            AsegurarRepositorios();
            return _productoRepository!;
        }

        public static ContratoRepository GetContratoRepository()
        {
            AsegurarRepositorios();
            return _contratoRepository!;
        }

        public static VentaRepository GetVentaRepository()
        {
            AsegurarRepositorios();
            return _ventaRepository!;
        }

        public static AuthService InitAuthService()
        {
            AsegurarRepositorios();
            return new AuthService(_adminRepository!, _empleadoRepository!);
        }

        public static VentaServices InitVentaServices()
        {
            AsegurarRepositorios();
            return new VentaServices(_ventaRepository!, _productoRepository!, _clienteRepository!);
        }

        public static AuthController InitAuthController(int maxIntentos = 3)
        {
            AuthService authService = InitAuthService();
            IConsoleWriter writer = InitConsoleWriter();
            return new AuthController(authService, writer, maxIntentos);
        }

        public static AdministradorController InitAdministradorController(Administrador? admin = null)
        {
            AsegurarRepositorios();
            IConsoleWriter writer = InitConsoleWriter();
            Administrador adminActual = admin ?? _adminRepository!.GetAll()[0];
            return new AdministradorController(adminActual, writer);
        }

        public static VendedorController InitVendedorController(Empleado vendedor)
        {
            AsegurarRepositorios();
            IConsoleWriter writer = InitConsoleWriter();
            return new VendedorController(vendedor, writer);
        }

        public static BodegueroController InitBodegueroController(Empleado bodeguero)
        {
            AsegurarRepositorios();
            IConsoleWriter writer = InitConsoleWriter();
            return new BodegueroController(bodeguero, writer);
        }
    }
}