
using System;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Util;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Commands.Bodega;

namespace TiendaCosmeticos.Controllers
{
    public class BodegueroController : IController
    {
        private readonly Empleado _bodeguero;
        private readonly IConsoleWriter _writer;
        private readonly BodegaServices _bodegaServices;

        // Constructor compatible con Startup.
        public BodegueroController(
            Empleado bodeguero,
            IConsoleWriter writer)
            : this(
                bodeguero,
                new BodegaServices(Startup.GetProductoRepository()),
                writer)
        {
        }

        // Constructor para inyectar el servicio.
        public BodegueroController(
            Empleado bodeguero,
            BodegaServices bodegaServices,
            IConsoleWriter writer)
        {
            _bodeguero = bodeguero
                ?? throw new ArgumentNullException(nameof(bodeguero));

            _bodegaServices = bodegaServices
                ?? throw new ArgumentNullException(nameof(bodegaServices));

            _writer = writer
                ?? throw new ArgumentNullException(nameof(writer));
        }

        public void DisplayMenu()
        {
            bool salir = false;

            while (!salir)
            {
                try
                {
                    _writer.Clear();
                    _writer.DisplayInitialMessage("MODULO DE BODEGA");

                    _writer.ShowMessage(
                        $"Bienvenido(a): {_bodeguero.Nombre} | Rol: Bodeguero\n");

                    _writer.DisplayMenu(
                        "Seleccione una opcion:",
                        "Registrar producto",
                        "Editar producto",
                        "Consultar productos",
                        "Cerrar sesion (Volver al login)");

                    string opcion = new Validator("Ingrese su opcion (1-4): ")
                        .NotEmpty()
                        .OnlyDigits()
                        .InputBetweenXandY(1, 4)
                        .ReadAndValidate();

                    switch (opcion)
                    {
                        case "1":
                            new AgregarProductoCommand(
                                _writer, _bodegaServices).Execute();
                            break;

                        case "2":
                            new EditarProductoCommand(
                                _writer, _bodegaServices).Execute();
                            break;

                        case "3":
                            new BuscarProductoCommand(
                                _writer, _bodegaServices).Execute();
                            break;

                        case "4":
                            salir = true;
                            break;
                    }
                }
                catch (TiendaException ex)
                {
                    _writer.ShowError(ex.Message);
                    _writer.WaitForKey();
                }
                catch (Exception)
                {
                    _writer.ShowError(
                        "Ocurrio un error inesperado. Puede intentar nuevamente.");

                    _writer.WaitForKey();
                }
            }
        }
    }
}
