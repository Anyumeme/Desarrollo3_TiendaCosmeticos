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
        private readonly AdministradorService _administradorService;
        private readonly IConsoleWriter _writer;
        private readonly Dictionary<int, ICommand> _commands;

        public PersonalController(
            EmpleadoService empleadoService,
            ContratoService contratoService,
            AdministradorService administradorService,
            IConsoleWriter writer,
            Administrador? adminSolicitante = null)
        {
            _empleadoService = empleadoService;
            _contratoService = contratoService;
            _administradorService = administradorService;
            _writer = writer;

            _commands = new Dictionary<int, ICommand>
            {
                { 1, new AgregarPersonalCommand(_writer, _empleadoService, _administradorService) },
                { 2, new EditarPersonalCommand(_writer, _empleadoService, _administradorService) },
                { 3, new ActualizarSalarioCommand(_writer, _contratoService) },
                { 4, new ConsultarPlanillaCommand(_writer, _contratoService) },
                { 5, new CambiarEstadoPersonalCommand(_writer, _empleadoService, _administradorService, _contratoService, adminSolicitante) },
                { 6, new ListarPersonalCommand(_writer, _empleadoService, _administradorService) }
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
                    "Agregar personal (Vendedor / Bodeguero / Administrador)",
                    "Editar datos de personal",
                    "Actualizar salario de contrato",
                    "Consultar planilla de pagos",
                    "Cambiar estado de personal (Activar / Desactivar)",
                    "Buscar / Listar personal",
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
