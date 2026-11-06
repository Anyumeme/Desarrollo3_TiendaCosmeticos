using System;
using System.Collections.Generic;
using TiendaCosmeticos.Commands.Administrador.PersonalCommand;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Controllers
{
    public class PersonalController : IController, ICommand
    {
        private readonly EmpleadoService _empleadoService;
        private readonly ContratoService _contratoService;
        private readonly IConsoleWriter _writer;
        private readonly Dictionary<int, ICommand> _commands;

        public PersonalController(
            EmpleadoService empleadoService,
            ContratoService contratoService,
            IConsoleWriter writer)
        {
            _empleadoService = empleadoService;
            _contratoService = contratoService;
            _writer = writer;

            _commands = new Dictionary<int, ICommand>
            {
                { 1, new AgregarPersonalCommand(_writer, _empleadoService) },
                { 2, new EditarPersonalCommand(_writer, _empleadoService) },
                { 3, new ActualizarSalarioCommand(_writer, _contratoService) },
                { 4, new ConsultarPlanillaCommand(_writer, _contratoService) },
                { 5, new CambiarEstadoPersonalCommand(_writer, _empleadoService, _contratoService) },
                { 6, new ListarPersonalCommand(_writer, _empleadoService) }
            };
        }

        public void DisplayMenu()
        {
            bool exitMenu = false;

            do
            {
                _writer.Clear();
                _writer.DisplayInitialMessage("GESTION DEL PERSONAL");
                _writer.DisplayMenu(
                    "Seleccione una opcion:",
                    "Agregar empleado (Vendedor / Bodeguero)",
                    "Editar datos de empleado",
                    "Actualizar salario de contrato",
                    "Consultar planilla de pagos",
                    "Cambiar estado de empleado (Activar / Desactivar)",
                    "Buscar / Listar empleados",
                    "Volver al menu principal"
                );

                string input = new Validator("Ingrese su opcion (1-7): ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 7)
                    .ReadAndValidate();

                int option = int.Parse(input);

                if (option == 7)
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

