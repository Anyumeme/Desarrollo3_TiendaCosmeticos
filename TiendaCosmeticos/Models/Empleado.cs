using System;

namespace TiendaCosmeticos.Models
{
    public class Empleado : PerfilTienda, IAutentication
    {
        public string CorreoElectronico { get; private set; } = string.Empty;
        public string Direccion { get; private set; } = string.Empty;
        public string EstadoCivil { get; private set; } = string.Empty;

        public string User { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public Rol Rol { get; private set; }

        public Empleado() : base() { }

        public Empleado(
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
            Rol rol,
            bool estado = true)
            : base(id, cedula, nombre, numeroCelular, fechaNacimiento, genero, estado)
        {
            CorreoElectronico = correoElectronico;
            Direccion = direccion;
            EstadoCivil = estadoCivil;
            User = user;
            Password = password;
            Rol = rol;
        }

        public void ActualizarEmpleado(
            string nombre,
            int numeroCelular,
            Genero genero,
            DateTime fechaNacimiento,
            string correoElectronico,
            string direccion,
            string estadoCivil,
            string? nuevaCedula = null)
        {
            ActualizarDatosBase(nombre, numeroCelular, genero, fechaNacimiento, nuevaCedula);
            CorreoElectronico = correoElectronico;
            Direccion = direccion;
            EstadoCivil = estadoCivil;
        }

        public void ActualizarEstadoCivil(string estadoCivil)
        {
            EstadoCivil = estadoCivil;
        }

        public void ActualizarCredenciales(string? nuevoUser, string? nuevoPasswordHash)
        {
            if (!string.IsNullOrWhiteSpace(nuevoUser))
            {
                User = nuevoUser.Trim();
            }
            if (!string.IsNullOrWhiteSpace(nuevoPasswordHash))
            {
                Password = nuevoPasswordHash;
            }
        }

        public void AsignarRol(Rol nuevoRol)
        {
            Rol = nuevoRol;
        }
    }
}
