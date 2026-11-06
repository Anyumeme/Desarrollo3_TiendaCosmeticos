using System;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.ClienteCommand
{
    public class CambiarEstadoClienteCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly ClienteService _clienteService;

        public CambiarEstadoClienteCommand(IConsoleWriter writer, ClienteService clienteService)
        {
            _writer = writer;
            _clienteService = clienteService;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("CAMBIO DE ESTADO DE CLIENTE (ACTIVAR / DESACTIVAR)");

            // 1. Solicitar el ID o Cedula del cliente
            string criterio = new Validator("Ingrese el ID o Cedula del cliente: ")
                .NotEmpty()
                .ReadAndValidate();

            Cliente cliente;
            try
            {
                cliente = _clienteService.BuscarCliente(criterio);
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
                _writer.WaitForKey();
                return;
            }

            // 2. Mostrar ficha actual
            string estadoTexto = cliente.Estado ? "ACTIVO" : "INACTIVO";
            _writer.ShowMessage("\n--- Informacion del Cliente ---");
            _writer.ShowMessage($"ID:     {cliente.Id}");
            _writer.ShowMessage($"Cedula: {cliente.Cedula}");
            _writer.ShowMessage($"Nombre: {cliente.Nombre}");
            _writer.ShowMessage($"Estado: {estadoTexto}");

            // 3. Confirmacion segun el estado actual
            string pregunta = cliente.Estado
                ? $"\nEl cliente se encuentra ACTIVO. ¿Esta seguro de que desea DESACTIVARLO? (S/N): "
                : $"\nEl cliente se encuentra INACTIVO. ¿Esta seguro de que desea REACTIVARLO? (S/N): ";

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

            // 4. Conmutar estado
            try
            {
                if (cliente.Estado)
                {
                    _clienteService.DesactivarCliente(cliente.Id);
                    _writer.ShowMessage($"\nCliente '{cliente.Id}' desactivado exitosamente.");
                }
                else
                {
                    _clienteService.ActivarCliente(cliente.Id);
                    _writer.ShowMessage($"\nCliente '{cliente.Id}' reactivado exitosamente.");
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
