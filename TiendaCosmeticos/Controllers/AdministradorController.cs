using System;
using System.Collections.Generic;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Controllers
{
    public class AdministradorController : IController
    {
        private readonly Administrador _adminActual;
        private readonly IConsoleWriter _writer;
        private readonly Dictionary<int, ICommand> _commands;

        public AdministradorController(
            Administrador adminActual,
            PersonalController personalController,
            ClienteController clienteController,
            ProductoController productoController,
            VentaController ventaController,
            IConsoleWriter writer)
        {
            _adminActual = adminActual;
            _writer = writer;

            _commands = new Dictionary<int, ICommand>
            {
                { 1, personalController },
                { 2, clienteController },
                { 3, productoController },
                { 4, ventaController }
            };
        }

        public void DisplayMenu()
        {
            if (_adminActual == null || _adminActual.Rol != Rol.Administrador)
            {
                _writer.ShowError("Acceso denegado: Se requieren permisos de Administrador.");
                _writer.WaitForKey();
                return;
            }

            bool salir = false;
            do
            {
                _writer.Clear();
                _writer.DisplayInitialMessage("PANEL DE ADMINISTRACION GENERAL");
                _writer.ShowMessage($"Bienvenido(a): {_adminActual.Nombre} | Rol: Administrador\n");

                _writer.DisplayMenu(
                    "Seleccione una opcion:",
                    "Gestion del Personal",
                    "Gestion de Clientes",
                    "Gestion de Productos",
                    "Gestion de Ventas (Vender, Anular, Listar)",
                    "Auditoria y Consulta de Logs del Sistema",
                    "Cerrar sesion (Volver al menu principal)"
                );

                string input = new Validator("Ingrese su opcion: ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 6)
                    .ReadAndValidate();

                int opcion = int.Parse(input);

                if (opcion == 6)
                {
                    salir = true;
                }
                else if (opcion == 5)
                {
                    _writer.ShowMessage("\n[Modulo de Auditoria y Logs - Se activara en la persistencia]");
                    _writer.WaitForKey();
                }
                else if (_commands.TryGetValue(opcion, out ICommand? command))
                {
                    command.Execute();
                }

            } while (!salir);
        }
    }
}
