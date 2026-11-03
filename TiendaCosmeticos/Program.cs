using TiendaCosmeticos.Controllers;
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

            consoleWriter.ShowMessage("\nSesion finalizada. Gracias por utilizar el sistema.");
        }
    }
}
