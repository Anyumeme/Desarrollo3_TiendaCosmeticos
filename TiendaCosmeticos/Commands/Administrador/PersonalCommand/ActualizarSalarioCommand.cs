using System;
using System.Globalization;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.PersonalCommand
{
    public class ActualizarSalarioCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly ContratoService _contratoService;

        public ActualizarSalarioCommand(IConsoleWriter writer, ContratoService contratoService)
        {
            _writer = writer;
            _contratoService = contratoService;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("MODIFICACION DE SALARIO MENSUAL");

            // 1. Solicitar identificador del empleado
            string criterio = new Validator("Ingrese el ID o Cedula del empleado: ")
                .NotEmpty()
                .ReadAndValidate();

            Contrato contrato;
            try
            {
                contrato = _contratoService.BuscarContratoPorEmpleado(criterio);

                if (!contrato.Activo)
                {
                    _writer.ShowError($"El contrato del empleado '{contrato.Empleado?.Id}' se encuentra inactivo. Debe reactivarlo antes de modificar el salario.");
                    _writer.WaitForKey();
                    return;
                }
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
                _writer.WaitForKey();
                return;
            }

            // 2. Mostrar la informacion salarial actual
            _writer.ShowMessage("\n--- Informacion Laboral y Salarial ---");
            _writer.ShowMessage($"ID Empleado:    {contrato.Empleado?.Id}");
            _writer.ShowMessage($"Nombre:         {contrato.Empleado?.Nombre}");
            _writer.ShowMessage($"Cargo:          {contrato.Cargo}");
            _writer.ShowMessage($"Salario Actual: C$ {contrato.SalarioMensual:N2}");

            // 3. Solicitar el nuevo salario
            string salarioTexto = new Validator("\nIngrese el nuevo salario mensual en C$: ")
                .NotEmpty()
                .DecimalPositivo()
                .ReadAndValidate();

            decimal nuevoSalario = decimal.Parse(salarioTexto.Trim().Replace(',', '.'), CultureInfo.InvariantCulture);

            // 4. Confirmacion de la modificacion
            string confirmacion = new Validator($"\n¿Confirma actualizar el salario de C$ {contrato.SalarioMensual:N2} a C$ {nuevoSalario:N2}? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (confirmacion.Trim().ToUpper() != "S")
            {
                _writer.ShowMessage("\nActualizacion de salario cancelada.");
                _writer.WaitForKey();
                return;
            }

            // 5. Aplicar la actualizacion protegida con try/catch
            try
            {
                _contratoService.ActualizarSalario(criterio, nuevoSalario);
                _writer.ShowMessage($"\nSalario mensual actualizado exitosamente para el empleado '{contrato.Empleado?.Id}'.");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al actualizar el salario: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
