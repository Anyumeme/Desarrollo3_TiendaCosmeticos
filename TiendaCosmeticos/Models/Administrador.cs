using System;

namespace TiendaCosmeticos.Models
{
    public class Administrador : Empleado
    {
        public Administrador() : base() { }

        public Administrador(
            string id,
            string cedula,
            string nombre,
            int numeroCelular,
            DateTime fechaNacimiento,
            Genero genero,
            string correoElectronico,
            string direccion,
            string estadoCivil,
            string user,
            string password,
            bool estado = true)
            : base(
                id,
                cedula,
                nombre,
                numeroCelular,
                fechaNacimiento,
                genero,
                correoElectronico,
                direccion,
                estadoCivil,
                user,
                password,
                Rol.Administrador,
                estado)
        {
        }
    }
}
