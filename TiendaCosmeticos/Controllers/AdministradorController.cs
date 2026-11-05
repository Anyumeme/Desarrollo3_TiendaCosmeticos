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
        private readonly Dictionary<int, ICommand> _commands = new();

        public AdministradorController(
            Administrador adminActual,
            IConsoleWriter writer)
        {
            _adminActual = adminActual;
            _writer = writer;
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
                    "Gestion de Vendedores (Agregar, Editar, Estado, Listar)",
                    "Gestion de Bodegueros (Agregar, Editar, Estado, Listar)",
                    "Gestion de Clientes (Agregar, Editar, Estado, Listar)",
                    "Gestion de Productos (Agregar, Editar, Estado, Listar)",
                    "Gestion de Ventas (Editar Estado, Listar)",
                    "Consultar Planilla de Pagos Mensuales",
                    "Auditoria y Consulta de Logs del Sistema",
                    "Cerrar sesion (Volver al menu principal)"
                );

                string input = new Validator("Ingrese su opcion (1-8): ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 8)
                    .ReadAndValidate();

                int opcion = int.Parse(input);

                if (opcion == 8)
                {
                    salir = true;
                }
                else if (_commands.TryGetValue(opcion, out ICommand? command))
                {
                    command.Execute();
                }
                else
                {
                    _writer.ShowMessage("\n[Opcion en construccion]");
                    _writer.WaitForKey();
                }

            } while (!salir);
        }
    }
}
