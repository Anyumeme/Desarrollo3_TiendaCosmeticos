using System;

namespace TiendaCosmeticos.Models
{
    public class Cliente : PerfilTienda
    {
        public DateTime FechaRegistro { get; private set; }

        public Cliente() : base() { }

        public Cliente(
            string id,
            string cedula,
            string nombre,
            int numeroCelular,
            DateTime fechaNacimiento,
            Genero genero,
            DateTime? fechaRegistro = null,
            bool estado = true)
            : base(id, cedula, nombre, numeroCelular, fechaNacimiento, genero, estado)
        {
            FechaRegistro = fechaRegistro ?? DateTime.Now;
        }

        public void ActualizarCliente(
            string nombre,
            int numeroCelular,
            Genero genero,
            DateTime fechaNacimiento,
            string? nuevaCedula = null)
        {
            ActualizarDatosBase(nombre, numeroCelular, genero, fechaNacimiento, nuevaCedula);
        }
    }
}
