using System;
using System.Collections.Generic;
using TiendaCosmeticos.Commands.Administrador.ProductoCommand;
using TiendaCosmeticos.Commands.Bodega;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Controllers
{
    public class ProductoController : IController, ICommand
    {
        private readonly BodegaServices _bodegaServices;
        private readonly IConsoleWriter _writer;
        private readonly Dictionary<int, ICommand> _commands;

        public ProductoController(BodegaServices bodegaServices, IConsoleWriter writer)
        {
            _bodegaServices = bodegaServices;
            _writer = writer;

            _commands = new Dictionary<int, ICommand>
            {
                { 1, new AgregarProductoCommand(_writer, _bodegaServices) },
                { 2, new EditarProductoCommand(_writer, _bodegaServices) },
                { 3, new ListarProductoCommand(_writer, _bodegaServices) },
                { 4, new CambiarEstadoProductoCommand(_writer, _bodegaServices) }
            };
        }

        public void DisplayMenu()
        {
            bool exitMenu = false;

            do
            {
                _writer.Clear();
                _writer.DisplayInitialMessage("GESTION DE PRODUCTOS");
                _writer.DisplayMenu(
                    "Seleccione una opcion:",
                    "Registrar nuevo producto",
                    "Editar producto existente",
                    "Consultar catalogo y stock de productos",
                    "Cambiar estado de producto (Activar / Desactivar)",
                    "Volver al menu principal"
                );

                string input = new Validator("Ingrese su opcion (1-5): ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 5)
                    .ReadAndValidate();

                int option = int.Parse(input);

                if (option == 5)
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

