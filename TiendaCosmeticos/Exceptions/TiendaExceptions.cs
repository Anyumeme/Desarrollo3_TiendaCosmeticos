using System;

namespace TiendaCosmeticos.Exceptions
{
    // Excepcion base para controlar todos los errores de logica de la tienda
    public class TiendaException : Exception
    {
        public TiendaException(string mensaje) : base(mensaje) { }
    }

    // Cuando un empleado intenta entrar a un menu que no es de su rol
    public class AccesoDenegadoException : TiendaException
    {
        public AccesoDenegadoException(string mensaje = "Acceso denegado: No tiene permisos para ingresar a este modulo.")
            : base(mensaje) { }
    }

    // Por si un usuario dado de baja intenta loguearse
    public class UsuarioInactivoException : TiendaException
    {
        public UsuarioInactivoException(string usuario)
            : base($"El usuario '{usuario}' se encuentra inactivo. Comuniquese con el administrador.") { }
    }

    // Cuando se agotan los 3 intentos en el login
    public class CuentaBloqueadaException : TiendaException
    {
        public CuentaBloqueadaException(string mensaje = "Ha superado el limite maximo de 3 intentos. El acceso ha sido bloqueado.")
            : base(mensaje) { }
    }

    // Cuando se busca por ID o cedula y no existe en las listas
    public class RegistroNoEncontradoException : TiendaException
    {
        public RegistroNoEncontradoException(string entidad, string identificador)
            : base($"No se encontro ningun registro de {entidad} con el identificador '{identificador}'.") { }
    }

    // Para evitar modificar o vender algo que esta desactivado
    public class RegistroInactivoException : TiendaException
    {
        public RegistroInactivoException(string entidad, string identificador)
            : base($"El registro de {entidad} ('{identificador}') esta inactivo. Debe reactivarlo antes de realizar cambios.") { }
    }

    // Evita activar lo que ya esta activo o desactivar lo que ya esta inactivo
    public class EstadoYaAsignadoException : TiendaException
    {
        public EstadoYaAsignadoException(string entidad, bool estadoActual)
            : base($"El registro de {entidad} ya se encuentra {(estadoActual ? "activo" : "inactivo")}.") { }
    }

    // Para no desactivar un trabajador si tiene un contrato vigente
    public class DependenciaActivaException : TiendaException
    {
        public DependenciaActivaException(string motivo)
            : base($"No se puede desactivar el registro porque tiene dependencias activas: {motivo}") { }
    }

    // Validacion para que no se repitan cedulas en clientes ni empleados
    public class CedulaDuplicadaException : TiendaException
    {
        public CedulaDuplicadaException(string cedula)
            : base($"La cedula '{cedula}' ya pertenece a otro registro en el sistema.") { }
    }

    // Para que no existan dos usuarios con el mismo nombre de acceso
    public class UsuarioDuplicadoException : TiendaException
    {
        public UsuarioDuplicadoException(string usuario)
            : base($"El nombre de usuario '{usuario}' ya esta registrado. Elija uno diferente.") { }
    }

    // Control de edad: empleados >= 18 y clientes >= 16
    public class EdadMinimaException : TiendaException
    {
        public EdadMinimaException(string tipoPersona, int edadMinima, int edadActual)
            : base($"No se puede registrar al {tipoPersona}. La edad minima requerida es de {edadMinima} anios (edad actual: {edadActual} anios).") { }
    }

    // Para no vender mas unidades de las que hay en inventario ni permitir stock negativo
    public class StockInsuficienteException : TiendaException
    {
        public StockInsuficienteException(string producto, int stockDisponible, int cantidadSolicitada)
            : base($"Stock insuficiente para '{producto}'. Unidades disponibles: {stockDisponible}, unidades solicitadas: {cantidadSolicitada}.") { }
    }

    // Control sanitario para cosmeticos vencidos
    public class ProductoVencidoException : TiendaException
    {
        public ProductoVencidoException(string producto, DateTime fechaVencimiento)
            : base($"El producto '{producto}' esta vencido desde el {fechaVencimiento:dd/MM/yyyy}. Por seguridad sanitaria no se puede vender.") { }
    }

    // Regla minorista para evitar compras masivas de mas de 200 piezas
    public class LimiteVentaSuperadoException : TiendaException
    {
        public LimiteVentaSuperadoException(int cantidadSolicitada, int limiteMaximo = 200)
            : base($"La cantidad solicitada ({cantidadSolicitada} unidades) supera el limite maximo permitido por venta minorista ({limiteMaximo} unidades).") { }
    }
}

