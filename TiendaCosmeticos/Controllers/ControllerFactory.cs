using System;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Controllers
{
    public static class ControllerFactory
    {
        public static IController CrearControlador(IAutentication usuario)
        {
            switch (usuario.Rol)
            {
                case Rol.Administrador:
                    return Startup.InitAdministradorController((Administrador)usuario);

                case Rol.Vendedor:
                    return Startup.InitVendedorController((Empleado)usuario);

                case Rol.Bodeguero:
                    return Startup.InitBodegueroController((Empleado)usuario);

                default:
                    throw new NotSupportedException($"El rol '{usuario.Rol}' no tiene un modulo asignado.");
            }
        }
    }
}
