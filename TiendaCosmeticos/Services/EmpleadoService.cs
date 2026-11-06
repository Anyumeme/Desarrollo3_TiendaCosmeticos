using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;

namespace TiendaCosmeticos.Services
{
    public class EmpleadoService
    {
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly AdministradorRepository _administradorRepository;
        private readonly ClienteRepository _clienteRepository;
        private readonly ContratoRepository _contratoRepository;

        public EmpleadoService(
            EmpleadoRepository empleadoRepository,
            AdministradorRepository administradorRepository,
            ClienteRepository clienteRepository,
            ContratoRepository contratoRepository)
        {
            _empleadoRepository = empleadoRepository;
            _administradorRepository = administradorRepository;
            _clienteRepository = clienteRepository;
            _contratoRepository = contratoRepository;
        }

        private void ValidarCedulaUnica(string cedula)
        {
            string limpia = cedula.Trim();
            if (_administradorRepository.GetByCedula(limpia) != null ||
                _empleadoRepository.GetByCedula(limpia) != null ||
                _clienteRepository.GetByCedula(limpia) != null)
                throw new CedulaDuplicadaException(limpia);
        }

        private void ValidarCelularUnico(int celular)
        {
            if (_administradorRepository.GetByCelular(celular) != null ||
                _empleadoRepository.GetByCelular(celular) != null ||
                _clienteRepository.GetByCelular(celular) != null)
                throw new CelularDuplicadoException(celular);
        }

        public bool ExisteCelular(int celular)
        {
            return _administradorRepository.GetByCelular(celular) != null ||
                   _empleadoRepository.GetByCelular(celular) != null ||
                   _clienteRepository.GetByCelular(celular) != null;
        }

        private void ValidarUsuarioUnico(string user)
        {
            string limpio = user.Trim();
            if (_administradorRepository.GetByUser(limpio) != null ||
                _empleadoRepository.GetByUser(limpio) != null)
                throw new UsuarioDuplicadoException(limpio);
        }

        private void ValidarEdadMinima(DateTime fechaNacimiento, int edadMinima, string tipoPersona)
        {
            DateTime hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;

            if (edad < edadMinima)
                throw new EdadMinimaException(tipoPersona, edadMinima, edad);
        }

        // Registra un nuevo vendedor o bodeguero con su contrato y codigo independiente (VEN-0001 o BOD-0001)
        public Empleado RegistrarEmpleado(Rol rol, string cedula, string nombre, int celular,
            DateTime fechaNacimiento, Genero genero, string correo, string direccion,
            string estadoCivil, string user, string password, decimal salarioMensual)
        {
            string tipoEmpleado = rol == Rol.Vendedor ? "vendedor" : "bodeguero";
            ValidarEdadMinima(fechaNacimiento, 18, tipoEmpleado);
            ValidarCedulaUnica(cedula);
            ValidarUsuarioUnico(user);
            ValidarCelularUnico(celular);

            // Generar identificador correlativo unificado para todo el personal (EMP-0001, EMP-0002, etc.)
            int correlativo = _empleadoRepository.GetAll().Count + _administradorRepository.GetAll().Count + 1;
            string nuevoId = $"EMP-{correlativo:D4}";
            while (_empleadoRepository.GetById(nuevoId) != null || _administradorRepository.GetById(nuevoId) != null)
            {
                correlativo++;
                nuevoId = $"EMP-{correlativo:D4}";
            }

            // Crear y guardar el empleado
            Empleado empleado = new Empleado(nuevoId, cedula.Trim(), nombre.Trim(), celular,
                fechaNacimiento, genero, correo.Trim(), direccion.Trim(), estadoCivil.Trim(),
                user.Trim(), password, rol);
            _empleadoRepository.Add(empleado);

            // Crear y guardar su contrato laboral con el cargo correspondiente
            string cargo = rol == Rol.Vendedor ? "Vendedor" : "Encargado de Bodega";
            Contrato contrato = new Contrato(empleado, salarioMensual, cargo, DateTime.Today, true);
            _contratoRepository.Add(contrato);

            return empleado;
        }

        // Busca un empleado (vendedor o bodeguero) por su id o por su cedula
        public Empleado BuscarEmpleado(string criterio, Rol? rolEsperado = null)
        {
            string nombreEntidad = rolEsperado.HasValue ? rolEsperado.Value.ToString() : "Empleado";
            if (string.IsNullOrWhiteSpace(criterio))
                throw new RegistroNoEncontradoException(nombreEntidad, criterio ?? string.Empty);

            string texto = criterio.Trim();
            Empleado? empleado = _empleadoRepository.GetById(texto)
                              ?? _empleadoRepository.GetByCedula(texto);

            if (empleado == null || (rolEsperado.HasValue && empleado.Rol != rolEsperado.Value))
                throw new RegistroNoEncontradoException(nombreEntidad, texto);

            return empleado;
        }

        // Permite modificar los datos de un empleado activo
        public void ActualizarEmpleado(string id, string nuevoNombre, int nuevoCelular,
            Genero nuevoGenero, DateTime nuevaFechaNacimiento, string nuevoCorreo,
            string nuevaDireccion, string nuevoEstadoCivil, string? nuevaCedula = null)
        {
            Empleado empleado = BuscarEmpleado(id);
            string tipo = empleado.Rol.ToString();

            if (!empleado.Estado)
                throw new RegistroInactivoException(tipo, empleado.Id);

            ValidarEdadMinima(nuevaFechaNacimiento, 18, tipo.ToLower());

            if (!string.IsNullOrWhiteSpace(nuevaCedula) &&
                !string.Equals(nuevaCedula.Trim(), empleado.Cedula, StringComparison.OrdinalIgnoreCase))
            {
                ValidarCedulaUnica(nuevaCedula);
            }

            if (nuevoCelular > 0 && nuevoCelular != empleado.NumeroCelular)
            {
                ValidarCelularUnico(nuevoCelular);
            }

            empleado.ActualizarEmpleado(nuevoNombre, nuevoCelular, nuevoGenero,
                nuevaFechaNacimiento, nuevoCorreo, nuevaDireccion, nuevoEstadoCivil, nuevaCedula);
        }

        // Desactiva logicamente al empleado verificando que no tenga contrato activo
        public void DesactivarEmpleado(string id)
        {
            Empleado empleado = BuscarEmpleado(id);
            string tipo = empleado.Rol.ToString();

            if (!empleado.Estado)
                throw new EstadoYaAsignadoException(tipo, false);

            Contrato? contratoActivo = _contratoRepository.GetContratoActivo(empleado.Cedula);
            if (contratoActivo != null)
                throw new DependenciaActivaException($"El {tipo.ToLower()} tiene un contrato laboral vigente. Debe cancelarlo antes de desactivar.");

            empleado.Desactivar();
        }

        // Reactiva al empleado si estaba inactivo
        public void ActivarEmpleado(string id)
        {
            Empleado empleado = BuscarEmpleado(id);
            string tipo = empleado.Rol.ToString();

            if (empleado.Estado)
                throw new EstadoYaAsignadoException(tipo, true);

            empleado.Activar();
        }

        // Lista los empleados permitiendo filtrar por rol (Vendedor, Bodeguero o todos) y por estado
        public List<Empleado> ListarEmpleados(Rol? rol = null, string filtro = "Todos")
        {
            var empleados = _empleadoRepository.GetAll();
            if (rol.HasValue)
                empleados = empleados.Where(e => e.Rol == rol.Value).ToList();

            return filtro.ToLower() switch
            {
                "activos" or "soloactivos" => empleados.Where(e => e.Estado).ToList(),
                "inactivos" or "soloinactivos" => empleados.Where(e => !e.Estado).ToList(),
                _ => empleados
            };
        }
    }
}

