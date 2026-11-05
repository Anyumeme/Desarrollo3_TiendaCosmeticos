using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;

namespace TiendaCosmeticos.Services
{
    public class ContratoService
    {
        private readonly ContratoRepository _contratoRepository;
        private readonly EmpleadoRepository _empleadoRepository;

        public ContratoService(
            ContratoRepository contratoRepository,
            EmpleadoRepository empleadoRepository)
        {
            _contratoRepository = contratoRepository;
            _empleadoRepository = empleadoRepository;
        }

        // Busca un empleado por su id o cedula para localizar su contrato
        private Empleado ObtenerEmpleado(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
                throw new RegistroNoEncontradoException("Empleado", criterio ?? string.Empty);

            string texto = criterio.Trim();
            Empleado? empleado = _empleadoRepository.GetById(texto)
                              ?? _empleadoRepository.GetByCedula(texto);

            if (empleado == null)
                throw new RegistroNoEncontradoException("Empleado", texto);

            return empleado;
        }

        // Busca el contrato asociado a un empleado por su id o cedula
        public Contrato BuscarContratoPorEmpleado(string criterioEmpleado)
        {
            Empleado empleado = ObtenerEmpleado(criterioEmpleado);
            Contrato? contrato = _contratoRepository.GetByEmpleadoCedula(empleado.Cedula);

            if (contrato == null)
                throw new RegistroNoEncontradoException("Contrato", empleado.Id);

            return contrato;
        }

        // Cancela el contrato activo de un empleado (necesario antes de desactivar al empleado)
        public void CancelarContrato(string criterioEmpleado)
        {
            Contrato contrato = BuscarContratoPorEmpleado(criterioEmpleado);

            if (!contrato.Activo)
                throw new EstadoYaAsignadoException("Contrato", false);

            contrato.CancelarContrato();
        }

        // Reactiva el contrato laboral de un empleado activo
        public void ReactivarContrato(string criterioEmpleado)
        {
            Contrato contrato = BuscarContratoPorEmpleado(criterioEmpleado);

            if (contrato.Empleado != null && !contrato.Empleado.Estado)
                throw new RegistroInactivoException("Empleado", contrato.Empleado.Id);

            if (contrato.Activo)
                throw new EstadoYaAsignadoException("Contrato", true);

            contrato.ReactivarContrato();
        }

        // Permite modificar el salario mensual de un contrato vigente
        public void ActualizarSalario(string criterioEmpleado, decimal nuevoSalario)
        {
            if (nuevoSalario <= 0)
                throw new TiendaException("El salario mensual debe ser un monto mayor a cero.");

            Contrato contrato = BuscarContratoPorEmpleado(criterioEmpleado);

            if (!contrato.Activo)
                throw new RegistroInactivoException("Contrato", contrato.Empleado?.Id ?? string.Empty);

            contrato.ActualizarSalario(nuevoSalario);
        }

        // Lista los contratos permitiendo filtrar por todos, activos o inactivos
        public List<Contrato> ListarContratos(string filtro = "Todos")
        {
            var contratos = _contratoRepository.GetAll();
            return filtro.ToLower() switch
            {
                "activos" or "soloactivos" => contratos.Where(c => c.Activo).ToList(),
                "inactivos" or "soloinactivos" => contratos.Where(c => !c.Activo).ToList(),
                _ => contratos
            };
        }

        // Obtiene los contratos vigentes de los empleados activos para la planilla mensual
        public List<Contrato> ObtenerPlanillaActiva()
        {
            return _contratoRepository.GetAll()
                .Where(c => c.Activo && c.Empleado != null && c.Empleado.Estado)
                .ToList();
        }

        // Calcula el monto total mensual de la planilla de trabajadores activos
        public decimal CalcularTotalPlanilla()
        {
            return ObtenerPlanillaActiva().Sum(c => c.SalarioMensual);
        }
    }
}

