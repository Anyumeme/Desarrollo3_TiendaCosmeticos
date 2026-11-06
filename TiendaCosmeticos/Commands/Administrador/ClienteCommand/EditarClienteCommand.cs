using System;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.ClienteCommand
{
    public class EditarClienteCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly ClienteService _clienteService;

        public EditarClienteCommand(IConsoleWriter writer, ClienteService clienteService)
        {
            _writer = writer;
            _clienteService = clienteService;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("EDICION DE DATOS DE CLIENTE");

            // 1. Solicitar el ID o Cedula del cliente
            string criterio = new Validator("Ingrese el ID o Cedula del cliente a editar: ")
                .NotEmpty()
                .ReadAndValidate();

            Cliente cliente;
            try
            {
                cliente = _clienteService.BuscarCliente(criterio);

                if (!cliente.Estado)
                {
                    _writer.ShowError($"El cliente '{cliente.Id}' se encuentra inactivo. Debe reactivarlo antes de modificar sus datos.");
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

            // 2. Mostrar ficha actual
            _writer.ShowMessage("\n--- Datos Actuales del Cliente ---");
            _writer.ShowMessage($"ID (Inmutable):   {cliente.Id}");
            _writer.ShowMessage($"Cedula:           {cliente.Cedula}");
            _writer.ShowMessage($"Nombre:           {cliente.Nombre}");
            _writer.ShowMessage($"Celular:          {cliente.NumeroCelular}");
            _writer.ShowMessage($"Genero:           {cliente.Genero}");
            _writer.ShowMessage($"Fecha Nacimiento: {cliente.FechaNacimiento:dd/MM/yyyy}");
            _writer.ShowMessage($"Fecha Registro:   {cliente.FechaRegistro:dd/MM/yyyy}");

            _writer.ShowMessage("\n--- Ingrese los Nuevos Datos ---");

            // 3. Capturar nuevos datos
            string nuevoNombre = new Validator("Nuevo nombre completo: ")
                .NotEmpty()
                .SoloLetras()
                .MinLength(3)
                .MaxLength(50)
                .ReadAndValidate();

            string celularTexto = new Validator("Nuevo numero de celular: ")
                .NotEmpty()
                .NumeroCelular()
                .ReadAndValidate();
            int nuevoCelular = int.Parse(celularTexto.Trim());

            _writer.DisplayMenu(
                "Seleccione el nuevo genero:",
                "Femenino",
                "Masculino"
            );
            string generoOpcion = new Validator("Ingrese su opcion (1-2): ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 2)
                .ReadAndValidate();
            Genero nuevoGenero = generoOpcion == "1" ? Genero.Femenino : Genero.Masculino;

            // 4. Preguntar si desea cambiar la cedula
            string cambiarCedula = new Validator("\n¿Desea modificar el numero de cedula? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            string? nuevaCedula = null;
            DateTime nuevaFechaNacimiento = cliente.FechaNacimiento;

            if (cambiarCedula.Trim().ToUpper() == "S")
            {
                nuevaCedula = new Validator("Nueva cedula (ej. 001-120590-0001A): ")
                    .NotEmpty()
                    .FormatoCedula()
                    .ReadCedula(out DateTime fechaExtraida);

                nuevaFechaNacimiento = fechaExtraida;
                _writer.ShowMessage($"Nueva fecha de nacimiento asignada: {nuevaFechaNacimiento:dd/MM/yyyy}");
            }

            // 5. Confirmacion de guardado
            string confirmacion = new Validator("\n¿Confirma guardar los cambios del cliente? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (confirmacion.Trim().ToUpper() != "S")
            {
                _writer.ShowMessage("\nEdicion cancelada por el usuario.");
                _writer.WaitForKey();
                return;
            }

            // 6. Actualizar cliente
            try
            {
                _clienteService.ActualizarCliente(
                    cliente.Id,
                    nuevoNombre,
                    nuevoCelular,
                    nuevoGenero,
                    nuevaFechaNacimiento,
                    nuevaCedula
                );

                _writer.ShowMessage($"\nDatos del cliente '{cliente.Id}' actualizados correctamente.");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al actualizar: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
