using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;

namespace TiendaCosmeticos.Services
{
    public class ClienteService
    {
        private readonly ClienteRepository _clienteRepository;
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly AdministradorRepository _administradorRepository;

        public ClienteService(
            ClienteRepository clienteRepository,
            EmpleadoRepository empleadoRepository,
            AdministradorRepository administradorRepository)
        {
            _clienteRepository = clienteRepository;
            _empleadoRepository = empleadoRepository;
            _administradorRepository = administradorRepository;
        }

        private void ValidarCedulaUnica(string cedula)
        {
            string limpia = cedula.Trim();
            if (_administradorRepository.GetByCedula(limpia) != null ||
                _empleadoRepository.GetByCedula(limpia) != null ||
                _clienteRepository.GetByCedula(limpia) != null)
                throw new CedulaDuplicadaException(limpia);
        }

        private void ValidarEdadMinima(DateTime fechaNacimiento, int edadMinima)
        {
            DateTime hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;

            if (edad < edadMinima)
                throw new EdadMinimaException("cliente", edadMinima, edad);
        }

        // Registra un nuevo cliente con edad minima de 16 anios y codigo CLI-0001
        public Cliente RegistrarCliente(string cedula, string nombre, int celular,
            DateTime fechaNacimiento, Genero genero)
        {
            ValidarEdadMinima(fechaNacimiento, 16);
            ValidarCedulaUnica(cedula);

            // Generar identificador correlativo con formato CLI-0001
            int correlativo = _clienteRepository.GetAll().Count + 1;
            string nuevoId = $"CLI-{correlativo:D4}";
            while (_clienteRepository.GetById(nuevoId) != null)
            {
                correlativo++;
                nuevoId = $"CLI-{correlativo:D4}";
            }

            Cliente cliente = new Cliente(
                nuevoId,
                cedula.Trim(),
                nombre.Trim(),
                celular,
                fechaNacimiento,
                genero,
                DateTime.Now,
                true
            );

            _clienteRepository.Add(cliente);
            return cliente;
        }

        // Busca un cliente por su id o por su cedula
        public Cliente BuscarCliente(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
                throw new RegistroNoEncontradoException("Cliente", criterio ?? string.Empty);

            string texto = criterio.Trim();
            Cliente? cliente = _clienteRepository.GetById(texto)
                            ?? _clienteRepository.GetByCedula(texto);

            if (cliente == null)
                throw new RegistroNoEncontradoException("Cliente", texto);

            return cliente;
        }

        // Permite modificar los datos de un cliente activo
        public void ActualizarCliente(string id, string nuevoNombre, int nuevoCelular,
            Genero nuevoGenero, DateTime nuevaFechaNacimiento, string? nuevaCedula = null)
        {
            Cliente cliente = BuscarCliente(id);

            if (!cliente.Estado)
                throw new RegistroInactivoException("Cliente", cliente.Id);

            ValidarEdadMinima(nuevaFechaNacimiento, 16);

            if (!string.IsNullOrWhiteSpace(nuevaCedula) &&
                !string.Equals(nuevaCedula.Trim(), cliente.Cedula, StringComparison.OrdinalIgnoreCase))
            {
                ValidarCedulaUnica(nuevaCedula);
            }

            cliente.ActualizarCliente(nuevoNombre, nuevoCelular, nuevoGenero, nuevaFechaNacimiento, nuevaCedula);
        }

        // Desactiva logicamente al cliente
        public void DesactivarCliente(string id)
        {
            Cliente cliente = BuscarCliente(id);

            if (!cliente.Estado)
                throw new EstadoYaAsignadoException("Cliente", false);

            cliente.Desactivar();
        }

        // Reactiva a un cliente que se encontraba inactivo
        public void ActivarCliente(string id)
        {
            Cliente cliente = BuscarCliente(id);

            if (cliente.Estado)
                throw new EstadoYaAsignadoException("Cliente", true);

            cliente.Activar();
        }

        // Lista los clientes permitiendo filtrar por todos, activos o inactivos
        public List<Cliente> ListarClientes(string filtro = "Todos")
        {
            var clientes = _clienteRepository.GetAll();
            return filtro.ToLower() switch
            {
                "activos" or "soloactivos" => clientes.Where(c => c.Estado).ToList(),
                "inactivos" or "soloinactivos" => clientes.Where(c => !c.Estado).ToList(),
                _ => clientes
            };
        }
    }
}

