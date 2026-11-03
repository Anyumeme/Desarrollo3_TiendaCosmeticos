using System;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Controllers
{
    public class BodegueroController : IController
    {
        private readonly Empleado _bodeguero;
        private readonly IConsoleWriter _writer;

        public BodegueroController(Empleado bodeguero, IConsoleWriter writer)
        {
            _bodeguero = bodeguero;
            _writer = writer;
        }

        public void DisplayMenu()
        {
            _writer.Clear();
            _writer.DisplayInitialMessage("MODULO DE BODEGA");
            _writer.ShowMessage($"Bienvenido(a): {_bodeguero.Nombre} | Rol: Bodeguero\n");
            _writer.ShowMessage("==================================================");
            _writer.ShowMessage("           MODULO EN CONSTRUCCION                 ");
            _writer.ShowMessage("==================================================");
            _writer.WaitForKey();
        }
    }
}
