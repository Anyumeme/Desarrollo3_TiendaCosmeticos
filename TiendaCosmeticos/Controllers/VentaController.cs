using System;
using System.Collections.Generic;
using TiendaCosmeticos.Commands.Ventas;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Controllers
{
    public class VentaController : IController, ICommand
    {
        private readonly Administrador _admin;
        private readonly VentaServices _ventaServices;
        private readonly BodegaServices _bodegaServices;
        private readonly ClienteService _clienteService;
        private readonly IConsoleWriter _writer;
        private readonly Dictionary<int, ICommand> _commands;

        public VentaController(
            Administrador admin,
            VentaServices ventaServices,
            BodegaServices bodegaServices,
            ClienteService clienteService,
            IConsoleWriter writer)
        {
            _admin = admin;
            _ventaServices = ventaServices;
            _bodegaServices = bodegaServices;
            _clienteService = clienteService;
            _writer = writer;

            _commands = new Dictionary<int, ICommand>
            {
                { 1, new RealizarVentaCommand(_writer, _ventaServices, _bodegaServices, _clienteService, _admin) },
                { 2, new ConsultarVentasCommand(_writer, _ventaServices) },
                { 3, new AnularVentaCommand(_writer, _ventaServices) }
            };
        }

        public void DisplayMenu()
        {
            bool exitMenu = false;

            do
            {
                _writer.Clear();
                _writer.DisplayInitialMessage("GESTION DE VENTAS");
                _writer.DisplayMenu(
                    "Seleccione una opcion:",
                    "Realizar nueva venta",
                    "Consultar historial de ventas",
                    "Anular factura de venta",
                    "Volver al menu principal"
                );

                string input = new Validator("Ingrese su opcion (1-4): ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 4)
                    .ReadAndValidate();

                int option = int.Parse(input);

                if (option == 4)
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

