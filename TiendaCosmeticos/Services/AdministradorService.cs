using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;

namespace TiendaCosmeticos.Services
{
    public class AdministradorService
    {
        private readonly AdministradorRepository _administradorRepository;
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly ClienteRepository _clienteRepository;

        public AdministradorService(
            AdministradorRepository administradorRepository,
            EmpleadoRepository empleadoRepository,
            ClienteRepository clienteRepository)
        {
            _administradorRepository = administradorRepository;
            _empleadoRepository = empleadoRepository;
            _clienteRepository = clienteRepository;
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

        public Administrador GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new RegistroNoEncontradoException("Administrador", id ?? string.Empty);

            Administrador? adm = _administradorRepository.GetById(id.Trim());
            if (adm == null)
                throw new RegistroNoEncontradoException("Administrador", id);

            return adm;
        }

        public void CrearAdministrador(Administrador administrador)
        {
            if (administrador == null)
                throw new TiendaException("Los datos del administrador no pueden ser nulos.");

            ValidarCedulaUnica(administrador.Cedula);
            ValidarUsuarioUnico(administrador.User);
            ValidarCelularUnico(administrador.NumeroCelular);
            _administradorRepository.Add(administrador);
        }

        public void DesactivarAdministrador(string id, Administrador? adminSolicitante = null)
        {
            // Regla de jerarquia: Solo Victor tiene permisos para desactivar a otros administradores
            if (adminSolicitante != null && !string.Equals(adminSolicitante.User, "Victor", StringComparison.OrdinalIgnoreCase))
                throw new TiendaException("Acceso denegado: Solo el Administrador Principal 'Victor' tiene permisos para dar de baja cuentas de Administrador.");

            Administrador admin = GetById(id);

            // Validar que no se desactive a Victor ni la propia cuenta en uso
            if (string.Equals(admin.User, "Victor", StringComparison.OrdinalIgnoreCase))
                throw new TiendaException("El administrador principal 'Victor' no puede ser desactivado.");

            if (adminSolicitante != null && string.Equals(admin.Id, adminSolicitante.Id, StringComparison.OrdinalIgnoreCase))
                throw new TiendaException("No puede desactivar su propia cuenta mientras la esta utilizando.");

            if (!admin.Estado)
                throw new EstadoYaAsignadoException("Administrador", false);

            admin.Desactivar();
        }

        public void ActivarAdministrador(string id, Administrador? adminSolicitante = null)
        {
            // Regla de jerarquia: Solo Victor tiene permisos para reactivar a otros administradores
            if (adminSolicitante != null && !string.Equals(adminSolicitante.User, "Victor", StringComparison.OrdinalIgnoreCase))
                throw new TiendaException("Acceso denegado: Solo el Administrador Principal 'Victor' tiene permisos para reactivar cuentas de Administrador.");

            Administrador admin = GetById(id);
            if (admin.Estado)
                throw new EstadoYaAsignadoException("Administrador", true);

            admin.Activar();
        }

        public void ActualizarAdministrador(string id, string nuevoNombre, int nuevoCelular,
            Genero nuevoGenero, DateTime nuevaFechaNacimiento, string nuevoCorreo,
            string nuevaDireccion, string nuevoEstadoCivil, string? nuevaCedula = null)
        {
            Administrador admin = GetById(id);
            if (!admin.Estado)
                throw new RegistroInactivoException("Administrador", admin.Id);

            if (!string.IsNullOrWhiteSpace(nuevaCedula) &&
                !string.Equals(nuevaCedula.Trim(), admin.Cedula, StringComparison.OrdinalIgnoreCase))
            {
                ValidarCedulaUnica(nuevaCedula);
            }

            if (nuevoCelular > 0 && nuevoCelular != admin.NumeroCelular)
            {
                ValidarCelularUnico(nuevoCelular);
            }

            admin.ActualizarEmpleado(nuevoNombre, nuevoCelular, nuevoGenero, nuevaFechaNacimiento,
                nuevoCorreo, nuevaDireccion, nuevoEstadoCivil, nuevaCedula);
        }

        // Busca un administrador por su id o por su cedula
        public Administrador BuscarAdministrador(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
                throw new RegistroNoEncontradoException("Administrador", criterio ?? string.Empty);

            string texto = criterio.Trim();
            Administrador? adm = _administradorRepository.GetById(texto)
                              ?? _administradorRepository.GetByCedula(texto);

            if (adm == null)
                throw new RegistroNoEncontradoException("Administrador", texto);

            return adm;
        }

        // Lista los administradores con opcion de filtrar por activos o inactivos
        public List<Administrador> ListarAdministradores(string filtro = "Todos")
        {
            var administradores = _administradorRepository.GetAll();
            return filtro.ToLower() switch
            {
                "activos" or "soloactivos" => administradores.Where(a => a.Estado).ToList(),
                "inactivos" or "soloinactivos" => administradores.Where(a => !a.Estado).ToList(),
                _ => administradores
            };
        }
    }
}
