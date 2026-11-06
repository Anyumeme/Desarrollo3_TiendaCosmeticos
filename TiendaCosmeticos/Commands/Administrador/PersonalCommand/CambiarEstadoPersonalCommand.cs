using System;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.Personal
{
    public class CambiarEstadoPersonalCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly EmpleadoService _empleadoService;
        private readonly ContratoService? _contratoService;

        public CambiarEstadoPersonalCommand(
            IConsoleWriter writer,
            EmpleadoService empleadoService,
            ContratoService? contratoService = null)
        {
            _writer = writer;
            _empleadoService = empleadoService;
            _contratoService = contratoService;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("CAMBIO DE ESTADO DE PERSONAL (ACTIVAR / DESACTIVAR)");

            // 1. Solicitar el ID o Cedula del empleado a conmutar
            string criterio = new Validator("Ingrese el ID o Cedula del empleado: ")
                .NotEmpty()
                .ReadAndValidate();

            Empleado empleado;
            try
            {
                empleado = _empleadoService.BuscarEmpleado(criterio);
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
                _writer.WaitForKey();
                return;
            }

            // 2. Mostrar la ficha del empleado
            string estadoActualTexto = empleado.Estado ? "ACTIVO" : "INACTIVO";
            string accionDeseada = empleado.Estado ? "DESACTIVAR" : "REACTIVAR";

            _writer.ShowMessage("\n--- Informacion del Empleado ---");
            _writer.ShowMessage($"ID:         {empleado.Id}");
            _writer.ShowMessage($"Cargo:      {empleado.Rol}");
            _writer.ShowMessage($"Nombre:     {empleado.Nombre}");
            _writer.ShowMessage($"Cedula:     {empleado.Cedula}");
            _writer.ShowMessage($"Estado:     {estadoActualTexto}");

            // 3. Confirmacion de la accion segun el estado actual
            string pregunta = empleado.Estado
                ? $"\nEl empleado se encuentra ACTIVO. ¿Esta seguro de que desea DESACTIVARLO? (S/N): "
                : $"\nEl empleado se encuentra INACTIVO. ¿Esta seguro de que desea REACTIVARLO? (S/N): ";

            string confirmacion = new Validator(pregunta)
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (confirmacion.Trim().ToUpper() != "S")
            {
                _writer.ShowMessage("\nOperacion cancelada por el usuario.");
                _writer.WaitForKey();
                return;
            }

            // 4. Ejecucion segura con manejo de dependencias de contrato
            try
            {
                if (empleado.Estado)
                {
                    // Si se va a desactivar y tenemos el servicio de contratos, ofrecer cancelar el contrato
                    if (_contratoService != null)
                    {
                        try
                        {
                            var contrato = _contratoService.BuscarContratoPorEmpleado(empleado.Id);
                            if (contrato != null && contrato.Activo)
                            {
                                string cancelarContratoSN = new Validator("El empleado tiene un contrato laboral activo. ¿Desea cancelar el contrato para proceder con la baja? (S/N): ")
                                    .NotEmpty()
                                    .ConfirmacionSN()
                                    .ReadAndValidate();

                                if (cancelarContratoSN.Trim().ToUpper() == "S")
                                {
                                    _contratoService.CancelarContrato(empleado.Id);
                                    _writer.ShowMessage("Contrato laboral cancelado exitosamente.");
                                }
                                else
                                {
                                    _writer.ShowMessage("\nNo se puede desactivar al empleado sin cancelar previamente su contrato laboral.");
                                    _writer.WaitForKey();
                                    return;
                                }
                            }
                        }
                        catch (RegistroNoEncontradoException)
                        {
                            // Si no tiene contrato asociado, prosigue normalmente
                        }
                    }

                    _empleadoService.DesactivarEmpleado(empleado.Id);
                    _writer.ShowMessage($"\nEmpleado '{empleado.Id}' desactivado exitosamente.");
                }
                else
                {
                    _empleadoService.ActivarEmpleado(empleado.Id);
                    _writer.ShowMessage($"\nEmpleado '{empleado.Id}' reactivado exitosamente.");
                }
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al cambiar el estado: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
