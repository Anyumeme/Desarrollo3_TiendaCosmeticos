using System;
using System.Collections.Generic;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.ClienteCommand
{
    public class ListarClienteCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly ClienteService _clienteService;

        public ListarClienteCommand(IConsoleWriter writer, ClienteService clienteService)
        {
            _writer = writer;
            _clienteService = clienteService;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("CONSULTA Y LISTADO DE CLIENTES");

            // 1. Menu de seleccion de filtro
            _writer.DisplayMenu(
                "Seleccione el filtro a aplicar:",
                "Listar todos los clientes (Activos e Inactivos)",
                "Listar solo clientes activos",
                "Listar solo clientes inactivos"
            );

            string opcion = new Validator("Ingrese su opcion (1-3): ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 3)
                .ReadAndValidate();

            string filtro = opcion switch
            {
                "2" => "Activos",
                "3" => "Inactivos",
                _ => "Todos"
            };

            // 2. Consulta al servicio
            try
            {
                List<Cliente> clientes = _clienteService.ListarClientes(filtro);

                if (clientes.Count == 0)
                {
                    _writer.ShowMessage("\nNo se encontraron clientes con el filtro seleccionado.");
                    _writer.WaitForKey();
                    return;
                }

                // 3. Renderizado de tabla
                _writer.ShowMessage("\n" + new string('-', 98));
                _writer.ShowMessage(string.Format("{0,-10} | {1,-25} | {2,-18} | {3,-10} | {4,-12} | {5,-8}",
                    "ID", "NOMBRE", "CEDULA", "CELULAR", "REGISTRO", "ESTADO"));
                _writer.ShowMessage(new string('-', 98));

                foreach (var c in clientes)
                {
                    string estadoTexto = c.Estado ? "Activo" : "Inactivo";
                    string nombreFormateado = c.Nombre.Length > 25 ? c.Nombre.Substring(0, 22) + "..." : c.Nombre;

                    _writer.ShowMessage(string.Format("{0,-10} | {1,-25} | {2,-18} | {3,-10} | {4,-12} | {5,-8}",
                        c.Id,
                        nombreFormateado,
                        c.Cedula,
                        c.NumeroCelular,
                        c.FechaRegistro.ToString("dd/MM/yyyy"),
                        estadoTexto));
                }

                _writer.ShowMessage(new string('-', 98));
                _writer.ShowMessage($"Total de clientes encontrados: {clientes.Count}");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al listar clientes: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
