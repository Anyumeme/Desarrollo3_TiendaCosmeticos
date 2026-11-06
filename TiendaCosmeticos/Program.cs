using System;
using TiendaCosmeticos.Controllers;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos
{
    public class Program
    {
        static void Main(string[] args)
        {
            IConsoleWriter consoleWriter = Startup.InitConsoleWriter();
            AuthController authController = Startup.InitAuthController();

            bool continuar = true;

            while (continuar)
            {
                try
                {
                    IAutentication? usuario = authController.IniciarSesion();

                    if (usuario == null)
                    {
                        continuar = false;
                    }
                    else
                    {
                        IController controller = ControllerFactory.CrearControlador(usuario);
                        controller.DisplayMenu();
                    }
                }
                catch (TiendaException ex)
                {
                    consoleWriter.ShowError($"Aviso del sistema: {ex.Message}");
                    consoleWriter.WaitForKey();
                }
                catch (Exception ex)
                {
                    consoleWriter.ShowError($"Error inesperado: {ex.Message}");
                    consoleWriter.WaitForKey();
                }
            }

            consoleWriter.Clear();
            consoleWriter.DisplayInitialMessage("SESION FINALIZADA");
            consoleWriter.ShowMessage("\nGracias por utilizar el Sistema de Gestion de Tienda de Cosmeticos.");
            consoleWriter.ShowMessage("Hasta pronto.\n");
        }
    }
}
