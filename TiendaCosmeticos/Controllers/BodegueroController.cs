using System;
using System.Collections.Generic;
using TiendaCosmeticos.Commands.Bodega;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Controllers
{
    public class BodegueroController : IController
    {
        private readonly Empleado _bodeguero;
        private readonly BodegaServices _bodegaServices;
        private readonly IConsoleWriter _writer;
        private readonly Dictionary<int, ICommand> _commands;

        public BodegueroController(Empleado bodeguero, IConsoleWriter writer)
            : this(bodeguero, Startup.InitBodegaServices(), writer)
        {
        }

        public BodegueroController(Empleado bodeguero, BodegaServices bodegaServices, IConsoleWriter writer)
        {
            _bodeguero = bodeguero;
            _bodegaServices = bodegaServices;
            _writer = writer;

            _commands = new Dictionary<int, ICommand>
            {
                { 1, new AgregarProductoCommand(_writer, _bodegaServices) },
                { 2, new EditarProductoCommand(_writer, _bodegaServices) },
                { 3, new BuscarProductoCommand(_writer, _bodegaServices) }
            };
        }

        public void DisplayMenu()
        {
            bool salir = false;

            do
            {
                _writer.Clear();
                _writer.DisplayInitialMessage("MODULO DE BODEGA");
                _writer.ShowMessage($"Bienvenido(a): {_bodeguero.Nombre} | Rol: Bodeguero\n");

                _writer.DisplayMenu(
                    "Seleccione una opcion:",
                    "Registrar producto",
                    "Editar producto",
                    "Consultar productos",
                    "Cerrar sesion (Volver al login)"
                );

                string input = new Validator("Ingrese su opcion: ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 4)
                    .ReadAndValidate();

                int option = int.Parse(input);

                if (option == 4)
                {
                    salir = true;
                }
                else if (_commands.TryGetValue(option, out ICommand? command))
                {
                    command.Execute();
                }

            } while (!salir);
        }
    }
}
