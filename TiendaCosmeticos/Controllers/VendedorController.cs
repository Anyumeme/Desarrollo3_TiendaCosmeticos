using System;
using System.Collections.Generic;
using TiendaCosmeticos.Commands.Administrador.ClienteCommand;
using TiendaCosmeticos.Commands.Bodega;
using TiendaCosmeticos.Commands.Ventas;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Controllers
{
    public class VendedorController : IController
    {
        private readonly Empleado _vendedor;
        private readonly IConsoleWriter _writer;
        private readonly Dictionary<int, ICommand> _commands;

        public VendedorController(
            Empleado vendedor,
            VentaServices ventaServices,
            BodegaServices bodegaServices,
            ClienteService clienteService,
            IConsoleWriter writer)
        {
            _vendedor = vendedor;
            _writer = writer;

            _commands = new Dictionary<int, ICommand>
            {
                { 1, new RealizarVentaCommand(_writer, ventaServices, bodegaServices, clienteService, _vendedor) },
                { 2, new AgregarClienteCommand(_writer, clienteService) },
                { 3, new ListarClienteCommand(_writer, clienteService) },
                { 4, new BuscarProductoCommand(_writer, bodegaServices) },
                { 5, new ConsultarVentasCommand(_writer, ventaServices) }
            };
        }

        public void DisplayMenu()
        {
            bool salir = false;

            do
            {
                _writer.Clear();
                _writer.DisplayInitialMessage("MODULO DE VENTAS");
                _writer.ShowMessage($"Usuario: {_vendedor.Nombre} | Rol: Vendedor\n");

                _writer.DisplayMenu(
                    "Seleccione una opcion:",
                    "Realizar venta de productos",
                    "Registrar nuevo cliente",
                    "Buscar / Listar clientes",
                    "Buscar productos disponibles",
                    "Consultar historial de ventas",
                    "Cerrar sesion (Volver al login)"
                );

                string input = new Validator("Ingrese su opcion: ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 6)
                    .ReadAndValidate();

                int option = int.Parse(input);

                if (option == 6)
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
