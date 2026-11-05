using System;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Controllers
{
    public class VendedorController : IController
    {
        private readonly Empleado _vendedor;
        private readonly IConsoleWriter _writer;

        public VendedorController(Empleado vendedor, IConsoleWriter writer)
        {
            _vendedor = vendedor;
            _writer = writer;
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

                string opcion = new Validator("Ingrese su opcion (1-6): ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 6)
                    .ReadAndValidate();

                switch (opcion)
                {
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                        _writer.ShowMessage("\n[En construccion]");
                        _writer.WaitForKey();
                        break;
                    case "6":
                        salir = true;
                        break;
                }

            } while (!salir);
        }
    }
}

