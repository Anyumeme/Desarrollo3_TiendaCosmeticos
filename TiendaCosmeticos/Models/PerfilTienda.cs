using System;

namespace TiendaCosmeticos.Models
{
    public abstract class PerfilTienda
    {
        public string Id { get; protected set; } = string.Empty;
        public string Cedula { get; protected set; } = string.Empty;
        public string Nombre { get; protected set; } = string.Empty;
        public int NumeroCelular { get; protected set; }
        public DateTime FechaNacimiento { get; protected set; }
        public Genero Genero { get; protected set; }
        public bool Estado { get; protected set; } = true;

        protected PerfilTienda() { }

        protected PerfilTienda(
            string id,
            string cedula,
            string nombre,
            int numeroCelular,
            DateTime fechaNacimiento,
            Genero genero,
            bool estado = true)
        {
            Id = id;
            Cedula = cedula;
            Nombre = nombre;
            NumeroCelular = numeroCelular;
            FechaNacimiento = fechaNacimiento;
            Genero = genero;
            Estado = estado;
        }

        public virtual void Desactivar()
        {
            Estado = false;
        }

        public virtual void Activar()
        {
            Estado = true;
        }

        public int CalcularEdad()
        {
            DateTime hoy = DateTime.Today;
            int edad = hoy.Year - FechaNacimiento.Year;
            if (FechaNacimiento.Date > hoy.AddYears(-edad))
            {
                edad--;
            }
            return Math.Max(0, edad);
        }

        public virtual void ActualizarDatosBase(
            string nombre,
            int numeroCelular,
            Genero genero,
            DateTime fechaNacimiento,
            string? nuevaCedula = null)
        {
            Nombre = nombre;
            NumeroCelular = numeroCelular;
            Genero = genero;
            FechaNacimiento = fechaNacimiento;

            if (!string.IsNullOrWhiteSpace(nuevaCedula))
            {
                Cedula = nuevaCedula;
            }
        }
    }
}
