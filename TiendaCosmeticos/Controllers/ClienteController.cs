using System;
using System.Collections.Generic;
using TiendaCosmeticos.Commands.Administrador.ClienteCommand;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Controllers
{
    public class ClienteController : IController, ICommand
    {
        private readonly ClienteService _clienteService;
        private readonly IConsoleWriter _writer;
        private readonly VentaServices _ventaServices;
        private readonly Dictionary<int, ICommand> _commands;

        public ClienteController(ClienteService clienteService, IConsoleWriter writer, VentaServices ventaServices)
        {
            _clienteService = clienteService;
            _writer = writer;
            _ventaServices = ventaServices;

            _commands = new Dictionary<int, ICommand>
            {
                { 1, new AgregarClienteCommand(_writer, _clienteService) },
                { 2, new EditarClienteCommand(_writer, _clienteService) },
                { 3, new CambiarEstadoClienteCommand(_writer, _clienteService) },
                { 4, new ConsultarComprasCommand(_writer, _clienteService, _ventaServices) },
                { 5, new ListarClienteCommand(_writer, _clienteService) }
            };
        }

        public void DisplayMenu()
        {
            bool exitMenu = false;
            do
            {
                _writer.Clear();
                _writer.DisplayInitialMessage("GESTION DE CLIENTES");
                _writer.DisplayMenu(
                    "Seleccione una opcion:",
                    "Agregar Cliente",
                    "Editar Cliente",
                    "Cambiar Estado del Cliente",
                    "Consultar Historial de Compras",
                    "Listar Clientes",
                    "Volver al menu principal"
                );

                string input = new Validator("Ingrese su opcion: ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 6)
                    .ReadAndValidate();

                int option = int.Parse(input);

                if (option == 6)
                {
                    exitMenu = true;
                }
                else if (_commands.TryGetValue(option, out ICommand? command))
                {
                    command.Execute();
                }

            } while (!exitMenu);
        }

        public void Execute()
        {
            DisplayMenu();
        }

        public void Undo()
        {
        }
    }
}
